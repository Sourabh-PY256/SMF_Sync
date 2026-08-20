using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer;
using EWP.SF.KafkaSync.DataAccess;
using Moq;

namespace EWP.SF.KafkaSync.Tests;

public class AssetOperationTests
{
    private readonly Mock<IAssetRepo> _assetRepo = new();
    private readonly Mock<IAttachmentOperation> _attachmentOperation = new();
    private readonly Mock<IActivityOperation> _activityOperation = new();
    private readonly Mock<ISchedulingCalendarShiftsOperation> _schedulingCalendarShiftsOperation = new();

    private AssetOperation CreateSut() => new(
        _assetRepo.Object,
        _attachmentOperation.Object,
        _activityOperation.Object,
        _schedulingCalendarShiftsOperation.Object);

    private static User UserWithPermissions(params string[] permissionCodes) => new()
    {
        Id = 1,
        Permissions = [.. permissionCodes.Select(code => new Permission { Code = code })]
    };

    [Fact]
    public async Task CreateAssetsExternal_FacilityType_RoutesToCreateFacility()
    {
        // Arrange
        var user = UserWithPermissions(Permissions.ASS_FACILITY_MANAGE);
        _assetRepo.Setup(x => x.GetFacility(It.IsAny<string>())).Returns((Facility?)null);
        _assetRepo
            .Setup(x => x.CreateFacility(It.IsAny<Facility>(), user, false, "Success"))
            .Returns(new ResponseData { IsSuccess = true, Code = "FAC-1" });
        var sut = CreateSut();
        var assets = new List<AssetExternal>
        {
            new() { AssetCode = "FAC-1", AssetName = "Plant 1", AssetType = "FACILITY", Status = "ACTIVE" }
        };

        // Act
        var result = await sut.CreateAssetsExternal(assets, [], user, false, "Success");

        // Assert
        Assert.Single(result);
        Assert.True(result[0].IsSuccess);
        _assetRepo.Verify(x => x.CreateFacility(It.Is<Facility>(f => f.Code == "FAC-1"), user, false, "Success"), Times.Once);
        _assetRepo.Verify(x => x.CreateFloor(It.IsAny<Floor>(), It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAssetsExternal_NewDisabledFacility_ReturnsFailureWithoutCallingRepo()
    {
        // Arrange - a facility that does not exist yet must arrive ACTIVE; a brand-new disabled
        // record is rejected before any create call is made.
        var user = UserWithPermissions(Permissions.ASS_FACILITY_MANAGE);
        _assetRepo.Setup(x => x.GetFacility(It.IsAny<string>())).Returns((Facility?)null);
        var sut = CreateSut();
        var assets = new List<AssetExternal>
        {
            new() { AssetCode = "FAC-NEW", AssetName = "New Plant", AssetType = "FACILITY", Status = "DISABLED" }
        };

        // Act
        var result = await sut.CreateAssetsExternal(assets, [], user, false, "Success");

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsSuccess);
        Assert.Contains("Disabled Facility", result[0].Message);
        _assetRepo.Verify(x => x.CreateFacility(It.IsAny<Facility>(), It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAssetsExternal_MissingPermission_ReturnsFailureForThatRecordOnly()
    {
        // Arrange - one record without permission should not abort the whole batch; the
        // UnauthorizedAccessException must be caught per-item, matching the try/catch-per-record
        // pattern used for every other entity in the Kafka processor.
        var user = UserWithPermissions(); // no permissions granted
        _assetRepo.Setup(x => x.GetFacility(It.IsAny<string>())).Returns((Facility?)null);
        var sut = CreateSut();
        var assets = new List<AssetExternal>
        {
            new() { AssetCode = "FAC-1", AssetName = "Plant 1", AssetType = "FACILITY", Status = "ACTIVE" }
        };

        // Act
        var result = await sut.CreateAssetsExternal(assets, [], user, false, "Success");

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsSuccess);
    }

    [Fact]
    public async Task CreateAssetsExternal_MultipleAssetTypes_RoutesEachToItsOwnRepoMethod()
    {
        // Arrange
        var user = UserWithPermissions(Permissions.ASS_FACILITY_MANAGE, Permissions.ASS_FLOOR_MANAGE, Permissions.ASS_WORKCENTER_MANAGE);
        _assetRepo.Setup(x => x.GetFacility(It.IsAny<string>())).Returns((Facility?)null);
        _assetRepo.Setup(x => x.GetFloor(It.IsAny<string>())).Returns((Floor?)null);
        _assetRepo.Setup(x => x.GetWorkCenter(It.IsAny<string>())).Returns((WorkCenter?)null);
        _assetRepo.Setup(x => x.CreateFacility(It.IsAny<Facility>(), user, false, "Success")).Returns(new ResponseData { IsSuccess = true, Code = "FAC-1" });
        _assetRepo.Setup(x => x.CreateFloor(It.IsAny<Floor>(), user, false, "Success")).Returns(new ResponseData { IsSuccess = true, Code = "FLR-1" });
        _assetRepo.Setup(x => x.CreateWorkCenter(It.IsAny<WorkCenter>(), user, false, "Success")).Returns(new ResponseData { IsSuccess = true, Code = "WC-1" });
        var sut = CreateSut();
        var assets = new List<AssetExternal>
        {
            new() { AssetCode = "FAC-1", AssetName = "Plant", AssetType = "FACILITY", Status = "ACTIVE" },
            new() { AssetCode = "FLR-1", AssetName = "Floor 1", AssetType = "FLOOR", Status = "ACTIVE" },
            new() { AssetCode = "WC-1", AssetName = "Work Center 1", AssetType = "WORKCENTER", Status = "ACTIVE" }
        };

        // Act
        var result = await sut.CreateAssetsExternal(assets, [], user, false, "Success");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.True(r.IsSuccess));
        _assetRepo.Verify(x => x.CreateFacility(It.IsAny<Facility>(), user, false, "Success"), Times.Once);
        _assetRepo.Verify(x => x.CreateFloor(It.IsAny<Floor>(), user, false, "Success"), Times.Once);
        _assetRepo.Verify(x => x.CreateWorkCenter(It.IsAny<WorkCenter>(), user, false, "Success"), Times.Once);
    }
}
