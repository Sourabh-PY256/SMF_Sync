using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

public class AssetRepo : IAssetRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public AssetRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region ProductionLine
    public List<ProductionLine> ListProductionLines(DateTime? DeltaDate = null)
    {
        List<ProductionLine> returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ProductionLine_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddNull("_ProductionLineCode");
                command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    if (returnValue.IsNull())
                    {
                        returnValue = [];
                    }

                    ProductionLine element = new()
                    {
                        Id = rdr["Code"].ToStr(),
                        Code = rdr["Code"].ToStr(),
                        AssetTypeCode = rdr["AssetTypeCode"].ToStr(),
                        Description = rdr["Name"].ToStr(),
                        WorkingTime = rdr["WorkingTime"].ToInt32(),
                        Image = rdr["Picture"].ToStr(),
                        Icon = rdr["Icon"].ToStr(),
                        Location = rdr["Location"].ToStr(),
                        CreationDate = rdr["CreateDate"].ToDate(),
                        CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                        ParentId = rdr["ParentCode"].ToStr(),
                        ParentCode = rdr["ParentCode"].ToStr(),
                        ParentAssetTypeCode = rdr["AssetParentTypeCode"].ToStr(),
                        Status = (Status)rdr["Status"].ToInt32(),
                        LogDetailId = rdr["LogDetailId"].ToStr()
                    };

                    if (rdr["UpdateDate"].ToDate().Year >= 1900)
                    {
                        element.ModifyDate = rdr["UpdateDate"].ToDate();
                        element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
                    }

                    returnValue.Add(element);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }

    public ProductionLine GetProductionLine(string Code)
    {
        ProductionLine returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ProductionLine_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                _ = command.Parameters.AddWithValue("_ProductionLineCode", Code);
                command.Parameters.AddNull("_DeltaDate");
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new ProductionLine
                    {
                        Id = rdr["Code"].ToStr(),
                        ParentId = rdr["ParentCode"].ToStr(),
                        Description = rdr["Name"].ToStr(),
                        Code = rdr["Code"].ToStr(),
                        WorkingTime = rdr["WorkingTime"].ToInt32(),
                        Image = rdr["Picture"].ToStr(),
                        Icon = rdr["Icon"].ToStr(),
                        Location = rdr["Location"].ToStr(),
                        CreationDate = rdr["CreateDate"].ToDate(),
                        CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                        ParentCode = rdr["ParentCode"].ToStr(),
                        Status = (Status)rdr["Status"].ToInt32(),
                        LogDetailId = rdr["LogDetailId"].ToStr()
                    };

                    if (rdr["UpdateDate"].ToDate().Year >= 1900)
                    {
                        returnValue.ModifyDate = rdr["UpdateDate"].ToDate();
                        returnValue.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
                    }
                }

                _ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    DeviceLink element = new()
                    {
                        Id = rdr["Code"].ToStr(),
                        Name = rdr["Name"].ToStr(),
                        CreationDate = rdr["CreateDate"].ToDate()
                    };
                    if (!string.IsNullOrEmpty(rdr["Status"].ToStr()))
                    {
                        switch (rdr["Status"].ToStr().ToLowerInvariant())
                        {
                            case "active":
                                {
                                    element.Status = Status.Active;
                                    break;
                                }
                            case "disabled":
                                {
                                    element.Status = Status.Disabled;
                                    break;
                                }
                            case "deleted":
                                {
                                    element.Status = Status.Deleted;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        element.Status = Status.Active;
                    }
                    (returnValue.Devices ??= []).Add(element);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }

    public ResponseData CreateProductionLine(ProductionLine productionLineInfo, User systemOperator, bool Validation, string Level)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ProductionLine_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                _ = command.Parameters.AddWithValue("_IsValidation", Validation);
                _ = command.Parameters.AddWithValue("_Level", Level);
                _ = command.Parameters.AddWithValue("_Code", productionLineInfo.Code);
                command.Parameters.AddCondition("_Name", productionLineInfo?.Description, !string.IsNullOrEmpty(productionLineInfo?.Description), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Description"));
                _ = command.Parameters.AddWithValue("_Status", productionLineInfo?.Status.ToInt32());
                _ = command.Parameters.AddWithValue("_Icon", productionLineInfo?.Icon);
                command.Parameters.AddCondition("_Picture", productionLineInfo?.Image, !string.IsNullOrEmpty(productionLineInfo?.Image));
                _ = command.Parameters.AddWithValue("_WorkingTime", productionLineInfo?.WorkingTime);
                command.Parameters.AddCondition("_Location", productionLineInfo?.Location, !string.IsNullOrEmpty(productionLineInfo?.Location));
                command.Parameters.AddCondition("_Operator", systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Employee"));
                _ = command.Parameters.AddWithValue("_ParentCode", productionLineInfo.ParentCode);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new ResponseData
                    {
                        Action = (ActionDB)rdr["Action"].ToInt32(),
                        IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
                        Code = rdr["Code"].ToStr(),
                        Message = rdr["Message"].ToStr(),
                    };
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }

    public bool DeleteProductionLine(ProductionLine productionLineInfo, User SystemOperator)
    {
        bool returnValue = false;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_ProductionLine_DEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                command.Parameters.AddCondition("_Operator", SystemOperator?.Id, SystemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_ProductionLineId", productionLineInfo?.Id, !string.IsNullOrEmpty(productionLineInfo?.Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Production Line Id"));

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                _ = command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                returnValue = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }
    #endregion ProductionLine
    #region WorkCenter
    public WorkCenter GetWorkCenter(string WorkCenterCode)
    {
        WorkCenter returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_WorkCenter_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                _ = command.Parameters.AddWithValue("_WorkCenterCode", WorkCenterCode);
                command.Parameters.AddNull("_DeltaDate");
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    WorkCenter element = new()
                    {
                        ParentCode = rdr["ParentCode"].ToStr(),
                        Code = rdr["Code"].ToStr(),
                        Id = rdr["Code"].ToStr(),
                        Name = rdr["Name"].ToStr(),
                        Icon = rdr["Icon"].ToStr(),

                        CreationDate = rdr["CreateDate"].ToDate(),
                        CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                        Status = (Status)rdr["Status"].ToInt32(),
                        Children = [],
                        Image = rdr["Picture"].ToStr(),
                        LogDetailId = rdr["LogDetailId"].ToStr()
                    };

                    if (rdr["UpdateDate"].ToDate().Year > 1900)
                    {
                        element.ModifyDate = rdr["UpdateDate"].ToDate();
                        element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
                    }

                    returnValue = element;
                }

                _ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    if (returnValue is not null)
                    {
                        ProductionLine element = new()
                        {
                            ParentCode = rdr["ParentCode"].ToStr(),
                            AssetTypeCode = rdr["AssetTypeCode"].ToStr(),
                            Code = rdr["Code"].ToStr(),
                            Description = rdr["Name"].ToStr(),
                            Icon = rdr["Icon"].ToStr(),

                            CreationDate = rdr["CreateDate"].ToDate(),
                            CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                            Status = (Status)rdr["Status"].ToInt32()
                        };

                        //if (rdr["Status"].ToStr() != "")
                        //{
                        //    switch (rdr["Status"].ToStr().ToLower())
                        //    {
                        //        case "active":
                        //            {
                        //                element.Status = Status.Active;
                        //                break;
                        //            }
                        //        case "disabled":
                        //            {
                        //                element.Status = Status.Disabled;
                        //                break;
                        //            }
                        //        case "deleted":
                        //            {
                        //                element.Status = Status.Deleted;
                        //                break;
                        //            }
                        //    }
                        //}
                        //else
                        //{
                        //    element.Status = Status.Active;
                        //}

                        if (rdr["UpdateDate"].ToDate().Year > 1900)
                        {
                            element.ModifyDate = rdr["UpdateDate"].ToDate();
                            element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
                        }
                        returnValue.Children.Add(element);
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }

    public ResponseData CreateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator, bool Validation, string Level)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_WorkCenter_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                _ = command.Parameters.AddWithValue("_Code", WorkCenterInfo.Code);
                _ = command.Parameters.AddWithValue("_Name", WorkCenterInfo.Name);
                _ = command.Parameters.AddWithValue("_Icon", WorkCenterInfo.Icon);
                _ = command.Parameters.AddWithValue("_Status", WorkCenterInfo.Status.ToInt32());
                command.Parameters.AddCondition("_Picture", WorkCenterInfo.Image, !string.IsNullOrEmpty(WorkCenterInfo.Image));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                _ = command.Parameters.AddWithValue("_IsValidation", Validation);
                _ = command.Parameters.AddWithValue("_Level", Level);
                _ = command.Parameters.AddWithValue("_ParentCode", WorkCenterInfo.ParentCode);
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new ResponseData
                    {
                        Action = (ActionDB)rdr["Action"].ToInt32(),
                        IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
                        Code = rdr["Code"].ToStr(),
                        Message = rdr["Message"].ToStr(),
                    };
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }
    #endregion WorkCenter
    #region floor
    public Floor GetFloor(string floorCode)
    {
        Floor returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Floor_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                _ = command.Parameters.AddWithValue("_FloorCode", floorCode);
                command.Parameters.AddNull("_DeltaDate");

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                // Cache column indices
                int codeOrdinal = rdr.GetOrdinal("Code");
                int parentCodeOrdinal = rdr.GetOrdinal("ParentCode");
                int nameOrdinal = rdr.GetOrdinal("Name");
                int iconOrdinal = rdr.GetOrdinal("Icon");
                int createDateOrdinal = rdr.GetOrdinal("CreateDate");
                int createUserOrdinal = rdr.GetOrdinal("CreateUser");
                int statusOrdinal = rdr.GetOrdinal("Status");
                int pictureOrdinal = rdr.GetOrdinal("Picture");
                int logDetailIdOrdinal = rdr.GetOrdinal("LogDetailId");
                int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    // Map data to the Floor object
                    Floor element = new()
                    {
                        Id = rdr[codeOrdinal].ToStr(),
                        ParentId = rdr[parentCodeOrdinal].ToStr(),
                        ParentCode = rdr[parentCodeOrdinal].ToStr(),
                        Code = rdr[codeOrdinal].ToStr(),
                        Name = rdr[nameOrdinal].ToStr(),
                        Icon = rdr[iconOrdinal].ToStr(),

                        CreationDate = rdr[createDateOrdinal].ToDate(),
                        CreatedBy = new User(rdr[createUserOrdinal].ToInt32()),
                        Status = (Status)rdr[statusOrdinal].ToInt32(),
                        Children = [],
                        Image = rdr[pictureOrdinal].ToStr(),
                        LogDetailId = rdr[logDetailIdOrdinal].ToStr()
                    };

                    // Check and map optional fields
                    if (rdr[updateDateOrdinal].ToDate().Year > 1900)
                    {
                        element.ModifyDate = rdr[updateDateOrdinal].ToDate();
                        element.ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32());
                    }

                    returnValue = element;
                }

                _ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    if (returnValue is not null)
                    {
                        WorkCenter element = new()
                        {
                            Id = rdr["Code"].ToStr(),
                            ParentCode = rdr["ParentCode"].ToStr(),
                            ParentId = rdr["ParentCode"].ToStr(),
                            Code = rdr["Code"].ToStr(),
                            Name = rdr["Name"].ToStr(),
                            Icon = rdr["Icon"].ToStr(),

                            CreationDate = rdr["CreateDate"].ToDate(),
                            CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                            Status = (Status)rdr["Status"].ToInt32()
                        };

                        if (rdr["UpdateDate"].ToDate().Year > 1900)
                        {
                            element.ModifyDate = rdr["UpdateUser"].ToDate();
                            element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
                        }
                        returnValue.Children.Add(element);
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }

    public ResponseData CreateFloor(Floor FloorInfo, User systemOperator, bool Validation, string Level)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Floor_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();
                _ = command.Parameters.AddWithValue("_Code", FloorInfo.Code);
                _ = command.Parameters.AddWithValue("_Name", FloorInfo.Name);
                _ = command.Parameters.AddWithValue("_Icon", FloorInfo.Icon);
                _ = command.Parameters.AddWithValue("_Status", FloorInfo.Status.ToInt32());
                command.Parameters.AddCondition("_Picture", FloorInfo.Image, !string.IsNullOrEmpty(FloorInfo.Image));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                _ = command.Parameters.AddWithValue("_IsValidation", Validation);
                _ = command.Parameters.AddWithValue("_Level", Level);
                _ = command.Parameters.AddWithValue("_ParentCode", FloorInfo.ParentCode);
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new ResponseData
                    {
                        Action = (ActionDB)rdr["Action"].ToInt32(),
                        IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
                        Code = rdr["Code"].ToStr(),
                        Message = rdr["Message"].ToStr(),
                    };
                }
            }
            catch (Exception ex)
            {

                //logger.Error(ex); throw; 
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }
    #endregion floor
    #region facility
    public Facility GetFacility(string facilityCode)
    {
        Facility returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Facility_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                _ = command.Parameters.AddWithValue("_Code", facilityCode);
                command.Parameters.AddNull("_DeltaDate");

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int CodeOrdinal = rdr.GetOrdinal("Code");
                int NameOrdinal = rdr.GetOrdinal("Name");
                int IconOrdinal = rdr.GetOrdinal("Icon");
                int StreetOrdinal = rdr.GetOrdinal("Street");
                int ZipCodeOrdinal = rdr.GetOrdinal("ZipCode");
                int CityOrdinal = rdr.GetOrdinal("City");
                int StateProvinceOrdinal = rdr.GetOrdinal("StateProvince");
                int CountryCodeOrdinal = rdr.GetOrdinal("CountryCode");
                int EmailOrdinal = rdr.GetOrdinal("Email");
                int PhoneNumberOrdinal = rdr.GetOrdinal("PhoneNumber");
                int NumberOrdinal = rdr.GetOrdinal("Number");
                int RegionOrdinal = rdr.GetOrdinal("Region");
                int CreateDateOrdinal = rdr.GetOrdinal("CreateDate");
                int CreateUserOrdinal = rdr.GetOrdinal("CreateUser");
                int StatusOrdinal = rdr.GetOrdinal("Status");
                int PictureOrdinal = rdr.GetOrdinal("Picture");
                int SchedulingModelOrdinal = rdr.GetOrdinal("SchedulingModel");
                int DefaultLanguageOrdinal = rdr.GetOrdinal("DefaultLanguage");
                int LatitudeOrdinal = rdr.GetOrdinal("Latitude");
                int LongitudeOrdinal = rdr.GetOrdinal("Longitude");
                int PlanningModelOrdinal = rdr.GetOrdinal("PlanningModel");
                int ManualCoordinatesOrdinal = rdr.GetOrdinal("ManualCoordinates");
                int LogDetailIdOrdinal = rdr.GetOrdinal("LogDetailId");
                int UpdateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                int UpdateUserOrdinal = rdr.GetOrdinal("UpdateUser");
                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    Facility element = new()
                    {
                        Id = rdr[CodeOrdinal].ToStr(),
                        Code = rdr[CodeOrdinal].ToStr(),
                        Name = rdr[NameOrdinal].ToStr(),
                        Icon = rdr[IconOrdinal].ToStr(),
                        Street = rdr[StreetOrdinal].ToStr(),
                        ZipCode = rdr[ZipCodeOrdinal].ToStr(),
                        City = rdr[CityOrdinal].ToStr(),
                        StateProvince = rdr[StateProvinceOrdinal].ToStr(),
                        Country = rdr[CountryCodeOrdinal].ToStr(),
                        Email = rdr[EmailOrdinal].ToStr(),
                        PhoneNumber = rdr[PhoneNumberOrdinal].ToStr(),
                        Number = rdr[NumberOrdinal].ToStr(),
                        Region = rdr[RegionOrdinal].ToStr(),
                        CreationDate = rdr[CreateDateOrdinal].ToDate(),
                        CreatedBy = new User(rdr[CreateUserOrdinal].ToInt32()),
                        Status = (Status)rdr[StatusOrdinal].ToInt32(),
                        Children = [],
                        Image = rdr[PictureOrdinal].ToStr(),
                        SchedulingModel = rdr[SchedulingModelOrdinal].ToStr(),
                        DefaultLanguage = rdr[DefaultLanguageOrdinal].ToStr(),
                        Latitude = rdr[LatitudeOrdinal].ToDouble(),
                        Longitude = rdr[LongitudeOrdinal].ToDouble(),
                        PlanningModel = rdr[PlanningModelOrdinal].ToStr(),
                        ManualCoordinates = rdr[ManualCoordinatesOrdinal].ToInt32(),
                        LogDetailId = rdr[LogDetailIdOrdinal].ToStr()
                    };

                    if (rdr[UpdateDateOrdinal].ToDate().Year > 1900)
                    {
                        element.ModifyDate = rdr[UpdateDateOrdinal].ToDate();
                        element.ModifiedBy = new User(rdr[UpdateUserOrdinal].ToInt32());
                    }

                    returnValue = element;
                }

                _ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    // int FloorIdOrdinal = rdr.GetOrdinal("FloorId");
                    int ParentCodeOrdinal = rdr.GetOrdinal("ParentCode");
                    int FloorCodeOrdinal = rdr.GetOrdinal("Code");
                    int FloorNameOrdinal = rdr.GetOrdinal("Name");
                    int FloorIconOrdinal = rdr.GetOrdinal("Icon");
                    int FloorCreateDateOrdinal = rdr.GetOrdinal("CreateDate");
                    int FloorCreateUserOrdinal = rdr.GetOrdinal("CreateUser");
                    int FloorStatusOrdinal = rdr.GetOrdinal("Status");
                    int FloorUpdateDateOrdinal = rdr.GetOrdinal("UpdateDate");
                    int FloorUpdateUserOrdinal = rdr.GetOrdinal("UpdateUser");

                    if (returnValue is not null)
                    {
                        Floor element = new()
                        {
                            //Id = rdr[FloorIdOrdinal].ToStr(),
                            ParentCode = rdr[ParentCodeOrdinal].ToStr(),
                            Code = rdr[FloorCodeOrdinal].ToStr(),
                            Name = rdr[FloorNameOrdinal].ToStr(),
                            Icon = rdr[FloorIconOrdinal].ToStr(),

                            CreationDate = rdr[FloorCreateDateOrdinal].ToDate(),
                            CreatedBy = new User(rdr[FloorCreateUserOrdinal].ToInt32()),
                            Status = (Status)rdr[FloorStatusOrdinal].ToInt32()
                        };

                        if (rdr[FloorUpdateDateOrdinal].ToDate().Year > 1900)
                        {
                            element.ModifyDate = rdr[FloorUpdateDateOrdinal].ToDate();
                            element.ModifiedBy = new User(rdr[FloorUpdateUserOrdinal].ToInt32());
                        }

                        returnValue.Children.Add(element);
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }

    public ResponseData CreateFacility(Facility FacilityInfo, User systemOperator, bool Validation, string Level)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Facility_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddNull("_ParentId");
                //command.Parameters.AddWithValue("_FacilityId", FacilityInfo.Id);
                _ = command.Parameters.AddWithValue("_Code", FacilityInfo.Code);
                _ = command.Parameters.AddWithValue("_Name", FacilityInfo.Name);
                command.Parameters.AddCondition("_Icon", FacilityInfo.Icon, !string.IsNullOrEmpty(FacilityInfo.Icon));

                command.Parameters.AddCondition("_Street", FacilityInfo.Street, !string.IsNullOrEmpty(FacilityInfo.Street));
                command.Parameters.AddCondition("_ZipCode", FacilityInfo.ZipCode, !string.IsNullOrEmpty(FacilityInfo.ZipCode));
                command.Parameters.AddCondition("_City", FacilityInfo.City, !string.IsNullOrEmpty(FacilityInfo.City));
                command.Parameters.AddCondition("_StateProvince", FacilityInfo.StateProvince, !string.IsNullOrEmpty(FacilityInfo.StateProvince));
                command.Parameters.AddCondition("_CountryCode", FacilityInfo.Country, !string.IsNullOrEmpty(FacilityInfo.Country));
                command.Parameters.AddCondition("_Email", FacilityInfo.Email, !string.IsNullOrEmpty(FacilityInfo.Email));
                command.Parameters.AddCondition("_PhoneNumber", FacilityInfo.PhoneNumber, !string.IsNullOrEmpty(FacilityInfo.PhoneNumber));
                command.Parameters.AddCondition("_Number", FacilityInfo.Number, !string.IsNullOrEmpty(FacilityInfo.Number));
                command.Parameters.AddCondition("_Region", FacilityInfo.Region, !string.IsNullOrEmpty(FacilityInfo.Region));
                _ = command.Parameters.AddWithValue("_Status", FacilityInfo.Status.ToInt32());
                command.Parameters.AddCondition("_Picture", FacilityInfo.Image, !string.IsNullOrEmpty(FacilityInfo.Image));
                command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
                _ = command.Parameters.AddWithValue("_IsValidation", Validation);
                _ = command.Parameters.AddWithValue("_Level", Level);
                _ = command.Parameters.AddWithValue("_SchedulingModel", FacilityInfo.SchedulingModel);
                _ = command.Parameters.AddWithValue("_DefaultLanguage", FacilityInfo.DefaultLanguage);
                _ = command.Parameters.AddWithValue("_Latitude", FacilityInfo.Latitude);
                _ = command.Parameters.AddWithValue("_Longitude", FacilityInfo.Longitude);
                _ = command.Parameters.AddWithValue("_PlanningModel", FacilityInfo.PlanningModel);
                _ = command.Parameters.AddWithValue("_ManualCoordinates", FacilityInfo.ManualCoordinates);
                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                int ActionOrdinal = rdr.GetOrdinal("Action");
                int IsSuccessOrdinal = rdr.GetOrdinal("IsSuccess");
                int CodeOrdinal = rdr.GetOrdinal("Code");
                int MessageOrdinal = rdr.GetOrdinal("Message");

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new ResponseData
                    {
                        Action = (ActionDB)rdr[ActionOrdinal].ToInt32(),
                        IsSuccess = rdr[IsSuccessOrdinal].ToInt32().ToBool(),
                        Code = rdr[CodeOrdinal].ToStr(),
                        Message = rdr[MessageOrdinal].ToStr(),
                    };
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }
    #endregion facility
}