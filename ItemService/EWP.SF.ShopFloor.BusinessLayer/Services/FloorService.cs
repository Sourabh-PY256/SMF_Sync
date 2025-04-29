// using AutoMapper;
// using EWP.SF.ShopFloor.API.Data;
// using EWP.SF.ShopFloor.API.Models;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Caching.Distributed;
// using System.Text.Json;

// namespace EWP.SF.ShopFloor.BusinessLayer.Services{


// public class FloorService : IFloorService
// {
//     private readonly FloorDbContext _context;
//     private readonly IMapper _mapper;
//     private readonly IDistributedCache _cache;
//     private readonly ILogger<FloorService> _logger;
//     private const string CacheKeyPrefix = "floor_";

//     public FloorService(
//         FloorDbContext context,
//         IMapper mapper,
//         IDistributedCache cache,
//         ILogger<FloorService> logger)
//     {
//         _context = context ?? throw new ArgumentNullException(nameof(context));
//         _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
//         _cache = cache ?? throw new ArgumentNullException(nameof(cache));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//     }

//     public async Task<IEnumerable<Floor>> ListFloorAsync(DateTime? deltaDate = null)
//     {
//         try
//         {
//             string cacheKey = $"{CacheKeyPrefix}list";
//             if (deltaDate.HasValue)
//             {
//                 cacheKey += $"_{deltaDate.Value:yyyyMMddHHmmss}";
//             }

//             // Try to get from cache first
//             var cachedFloors = await _cache.GetStringAsync(cacheKey);
//             if (!string.IsNullOrEmpty(cachedFloors))
//             {
//                 return JsonSerializer.Deserialize<IEnumerable<Floor>>(cachedFloors);
//             }

//             // If not in cache, get from database
//             var query = _context.Floors
//                 .Include(f => f.Children)
//                 .Include(f => f.CreatedBy)
//                 .Include(f => f.ModifiedBy)
//                 .AsNoTracking();

//             if (deltaDate.HasValue)
//             {
//                 query = query.Where(f => f.ModifyDate >= deltaDate || f.CreationDate >= deltaDate);
//             }

//             var floors = await query.ToListAsync();

//             // Cache the results
//             var cacheOptions = new DistributedCacheEntryOptions
//             {
//                 AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
//             };
//             await _cache.SetStringAsync(
//                 cacheKey,
//                 JsonSerializer.Serialize(floors),
//                 cacheOptions);

//             return floors;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error retrieving floor list");
//             throw;
//         }
//     }

//     public async Task<Floor> GetFloorAsync(string code)
//     {
//         try
//         {
//             string cacheKey = $"{CacheKeyPrefix}{code}";

//             // Try to get from cache first
//             var cachedFloor = await _cache.GetStringAsync(cacheKey);
//             if (!string.IsNullOrEmpty(cachedFloor))
//             {
//                 return JsonSerializer.Deserialize<Floor>(cachedFloor);
//             }

//             // If not in cache, get from database
//             var floor = await _context.Floors
//                 .Include(f => f.Children)
//                 .Include(f => f.CreatedBy)
//                 .Include(f => f.ModifiedBy)
//                 .FirstOrDefaultAsync(f => f.Code == code);

//             if (floor != null)
//             {
//                 // Cache the result
//                 var cacheOptions = new DistributedCacheEntryOptions
//                 {
//                     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
//                 };
//                 await _cache.SetStringAsync(
//                     cacheKey,
//                     JsonSerializer.Serialize(floor),
//                     cacheOptions);
//             }

//             return floor;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error retrieving floor with code {Code}", code);
//             throw;
//         }
//     }

//     public async Task<Floor> CreateFloorAsync(Floor floor, string userId)
//     {
//         try
//         {
//             // Validate floor code uniqueness
//             if (await _context.Floors.AnyAsync(f => f.Code == floor.Code))
//             {
//                 throw new InvalidOperationException($"Floor with code {floor.Code} already exists");
//             }

//             floor.CreationDate = DateTime.UtcNow;
//             floor.CreatedBy = userId;
//             floor.Status = Status.Active;

//             _context.Floors.Add(floor);
//             await _context.SaveChangesAsync();

//             // Invalidate cache
//             await InvalidateCacheAsync(floor.Code);

//             return floor;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error creating floor {@Floor}", floor);
//             throw;
//         }
//     }

//     public async Task<Floor> UpdateFloorAsync(Floor floor, string userId)
//     {
//         try
//         {
//             var existingFloor = await _context.Floors
//                 .FirstOrDefaultAsync(f => f.Code == floor.Code);

//             if (existingFloor == null)
//             {
//                 throw new KeyNotFoundException($"Floor with code {floor.Code} not found");
//             }

//             // Update properties
//             existingFloor.Name = floor.Name;
//             existingFloor.ParentCode = floor.ParentCode;
//             existingFloor.ParentId = floor.ParentId;
//             existingFloor.Icon = floor.Icon;
//             existingFloor.ModifyDate = DateTime.UtcNow;
//             existingFloor.ModifiedBy = userId;
//             existingFloor.Status = floor.Status;
//             existingFloor.Image = floor.Image;
//             existingFloor.AssetType = floor.AssetType;

//             await _context.SaveChangesAsync();

//             // Invalidate cache
//             await InvalidateCacheAsync(floor.Code);

//             return existingFloor;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error updating floor {@Floor}", floor);
//             throw;
//         }
//     }

//     public async Task<bool> DeleteFloorAsync(Floor floor, string userId)
//     {
//         try
//         {
//             var existingFloor = await _context.Floors
//                 .Include(f => f.Children)
//                 .FirstOrDefaultAsync(f => f.Code == floor.Code);

//             if (existingFloor == null)
//             {
//                 return false;
//             }

//             // Check if floor has children
//             if (existingFloor.Children?.Any() == true)
//             {
//                 throw new InvalidOperationException("Cannot delete floor with existing children");
//             }

//             _context.Floors.Remove(existingFloor);
//             await _context.SaveChangesAsync();

//             // Invalidate cache
//             await InvalidateCacheAsync(floor.Code);

//             return true;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error deleting floor {@Floor}", floor);
//             throw;
//         }
//     }

//     public async Task SaveImageEntityAsync(string entityType, string image, string code, string userId)
//     {
//         try
//         {
//             var floor = await _context.Floors
//                 .FirstOrDefaultAsync(f => f.Code == code);

//             if (floor == null)
//             {
//                 throw new KeyNotFoundException($"Floor with code {code} not found");
//             }

//             floor.Image = image;
//             floor.ModifyDate = DateTime.UtcNow;
//             floor.ModifiedBy = userId;

//             await _context.SaveChangesAsync();

//             // Invalidate cache
//             await InvalidateCacheAsync(code);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error saving image for floor {Code}", code);
//             throw;
//         }
//     }

//     public async Task AttachmentSyncAsync(string attachmentId, string code, string userId)
//     {
//         try
//         {
//             var floor = await _context.Floors
//                 .FirstOrDefaultAsync(f => f.Code == code);

//             if (floor == null)
//             {
//                 throw new KeyNotFoundException($"Floor with code {code} not found");
//             }

//             if (floor.AttachmentIds == null)
//             {
//                 floor.AttachmentIds = new List<string>();
//             }

//             if (!floor.AttachmentIds.Contains(attachmentId))
//             {
//                 floor.AttachmentIds.Add(attachmentId);
//                 floor.ModifyDate = DateTime.UtcNow;
//                 floor.ModifiedBy = userId;

//                 await _context.SaveChangesAsync();

//                 // Invalidate cache
//                 await InvalidateCacheAsync(code);
//             }
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error syncing attachment {AttachmentId} for floor {Code}", attachmentId, code);
//             throw;
//         }
//     }

//     private async Task InvalidateCacheAsync(string code)
//     {
//         try
//         {
//             // Remove specific floor cache
//             await _cache.RemoveAsync($"{CacheKeyPrefix}{code}");
            
//             // Remove list caches
//             await _cache.RemoveAsync($"{CacheKeyPrefix}list");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error invalidating cache for floor {Code}", code);
//             // Don't throw here as this is a background operation
//         }
//     }
// }
// }