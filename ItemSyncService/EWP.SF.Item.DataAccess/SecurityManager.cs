using System.Data;
using System.Globalization;
using System.Text;

using EWP.SF.Item.BusinessEntities;
using EWP.SF.ConnectionModule;
using EWP.SF.Helper;
using EWP.SF.Common.Enumerators;
using MySqlConnector;
using EWP.SF.Common.Models;

namespace EWP.SF.Item.DataAccess;

public class SecurityManager
{
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
	public string ConnectionString { get; set; }

	public SecurityManager()
	{
		ConnectionString = ApplicationSettings.Instance.GetConnectionString();
	}

	// public async Task<RequestContext> CreateTokenAngular(UserCredentials user, CancellationToken cancellationToken = default)
	// {
	// 	ArgumentNullException.ThrowIfNull(user);

	// 	try
	// 	{
	// 		await using EWP_Connection connection = new(ConnectionString);
	// 		await using (connection.ConfigureAwait(false))
	// 		{
	// 			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

	// 			await using EWP_Command command = new("SP_SF_Token_INS", connection)
	// 			{
	// 				CommandType = CommandType.StoredProcedure
	// 			};

	// 			await using (command.ConfigureAwait(false))
	// 			{
	// 				_ = command.Parameters.AddWithValue("_JSON", user.ToJSON());

	// 				MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
	// 				await using (rdr.ConfigureAwait(false))
	// 				{
	// 					RequestContext result = new();

	// 					if (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
	// 					{
	// 						result.Token = rdr["Token"].ToStr();
	// 						result.User = new User(rdr["UserId"].ToInt32())
	// 						{
	// 							DisplayName = rdr["UserName"].ToStr(),
	// 							Email = rdr["UserEmail"].ToStr(),
	// 							Status = (Status)rdr["Status"].ToInt32(),
	// 							SecretKey = rdr["SecretAuth"].ToStr()
	// 						};
	// 						result.MultiSession = rdr["MultiSession"].ToBool();
	// 						result.LanguageCode = rdr["LangCode"].ToStr();
	// 						result.User.DefaultLayout = rdr["DefaultLayoutId"].ToInt32();
	// 						result.User.Role = new Role(rdr["RoleId"].ToInt32());
	// 						result.MainStartModule = rdr["ModulePath"].ToStr();
	// 						result.Avatar = rdr["Avatar"].ToStr();
	// 						result.TimeZoneOffset = rdr["Offset"].ToInt32();
	// 						result.Auth2FA = rdr["Auth2FA"].ToBool();
	// 						result.UsedMultiFactor = rdr["UsedMultiFactor"].ToInt32();
	// 						result.TemporalPassword = rdr["TemporalPassword"].ToBool();
	// 						result.DefaultMenu = rdr["DefaultMenu"].ToStr();
	// 						result.Employee = new EmployeeContext
	// 						{
	// 							Id = rdr["EmpId"].ToStr(),
	// 							Code = rdr["EmpCode"].ToStr(),
	// 							Name = rdr["EmpName"].ToStr(),
	// 							LastName = rdr["EmpLastName"].ToStr(),
	// 							FullName = rdr["EmpFullName"].ToStr(),
	// 							Email = rdr["EmpEmail"].ToStr(),
	// 							PositionCode = rdr["PositionCode"].ToStr(),
	// 							PositionName = rdr["PositionName"].ToStr(),
	// 							UserTypeCode = rdr["UserTypeCode"].ToStr(),
	// 						};
	// 					}

	// 					_ = await rdr.NextResultAsync(cancellationToken).ConfigureAwait(false);

	// 					int permissionCodeOrdinal = rdr.GetOrdinal("PermissionCode");
	// 					while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
	// 					{
	// 						Permission permission = new()
	// 						{
	// 							Code = rdr[permissionCodeOrdinal].ToStr()
	// 						};

	// 						(result.User.Permissions ??= []).Add(permission);
	// 					}

	// 					_ = await rdr.NextResultAsync(cancellationToken).ConfigureAwait(false);

	// 					int urlOrdinal = rdr.GetOrdinal("Url");
	// 					while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
	// 					{
	// 						ReqPermissions permission = new()
	// 						{
	// 							PermissionCode = rdr[urlOrdinal].ToStr()
	// 						};

	// 						(result.User.PermissionsModule ??= []).Add(permission);
	// 					}

	// 					return result;
	// 				}
	// 			}
	// 		}
	// 	}
	// 	catch
	// 	{
	// 		throw;
	// 	}
	// }

	

	public async Task<int> RemoveToken(string hash, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(hash);

		try
		{
			await using EWP_Connection connection = new(ConnectionString);
			await using (connection.ConfigureAwait(false))
			{
				await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

				await using EWP_Command command = new("SP_SF_Token_DEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				await using (command.ConfigureAwait(false))
				{
					command.Parameters.AddCondition("_Hash", hash, true, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "hash"));

					object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

					return result is int intValue ? intValue : 0;
				}
			}
		}
		catch
		{
			throw;
		}
	}

	public async Task<RequestContext> ValidateToken(string token, string ip, bool simple = false, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty("Token cannot be null or empty.");
		ArgumentException.ThrowIfNullOrEmpty("IP cannot be null or empty.");

		try
		{
			await using EWP_Connection connection = new(ConnectionString);
			await using (connection.ConfigureAwait(false))
			{
				await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

				await using EWP_Command command = new("SP_SF_Token_IsValid", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				await using (command.ConfigureAwait(false))
				{
					command.Parameters.Clear();
					_ = command.Parameters.AddWithValue("_Token", token);
					_ = command.Parameters.AddWithValue("_IP", ip);
					_ = command.Parameters.AddWithValue("_Simple", simple);

					RequestContext requestContext = new();

					await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (reader.ConfigureAwait(false))
					{
						if (simple)
						{
							if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								requestContext.Token = reader["Token"].ToStr();
							}
							return requestContext;
						}

						// Read user data
						if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
						{
							int tokenOrdinal = reader.GetOrdinal("Token");
							int userIdOrdinal = reader.GetOrdinal("UserId");
							int nameOrdinal = reader.GetOrdinal("Name");
							int emailOrdinal = reader.GetOrdinal("Email");
							int statusOrdinal = reader.GetOrdinal("Status");
							int hashOrdinal = reader.GetOrdinal("Hash");
							int multiSessionOrdinal = reader.GetOrdinal("MultiSession");
							int langCodeOrdinal = reader.GetOrdinal("LangCode");
							int defaultLayoutIdOrdinal = reader.GetOrdinal("DefaultLayoutId");
							int isPublicOrdinal = reader.GetOrdinal("IsPublic");
							int expirationDateOrdinal = reader.GetOrdinal("ExpirationDate");
							int browserOrdinal = reader.GetOrdinal("Browser");
							int osOrdinal = reader.GetOrdinal("OS");
							int locationOrdinal = reader.GetOrdinal("Location");
							int employeeCodeOrdinal = reader.GetOrdinal("EmployeeCode");
							int offsetOrdinal = reader.GetOrdinal("Offset");
							int secretAuthOrdinal = reader.GetOrdinal("SecretAuth");

							requestContext.Token = reader[tokenOrdinal].ToStr();
							requestContext.User = new User(reader[userIdOrdinal].ToInt32())
							{
								DisplayName = reader[nameOrdinal].ToStr(),
								Email = reader[emailOrdinal].ToStr(),
								Status = (Status)reader[statusOrdinal].ToInt32(),
								Hash = reader[hashOrdinal].ToStr(),
								SecretKey = reader[secretAuthOrdinal].ToStr()
							};

							requestContext.MultiSession = reader[multiSessionOrdinal].ToBool();
							requestContext.LanguageCode = reader[langCodeOrdinal].ToStr();
							requestContext.User.DefaultLayout = reader[defaultLayoutIdOrdinal].ToInt32();
							requestContext.IsPublic = reader[isPublicOrdinal].ToBool();
							requestContext.ExpirationDate = reader[expirationDateOrdinal].ToDate();
							requestContext.Browser = reader[browserOrdinal].ToStr();
							requestContext.OS = reader[osOrdinal].ToStr();
							requestContext.Location = reader[locationOrdinal].ToStr();
							requestContext.EmployeeCode = reader[employeeCodeOrdinal].ToStr();
							requestContext.User.EmployeeId = reader[employeeCodeOrdinal].ToStr();
							requestContext.TimeZoneOffset = reader[offsetOrdinal].ToDouble();
						}

						// Move to the next result set for permissions
						if (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false))
						{
							int roleIdOrdinal = reader.GetOrdinal("RoleId");
							int permissionCodeOrdinal = reader.GetOrdinal("PermissionCode");

							while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								int roleId = reader[roleIdOrdinal].ToInt32();
								Permission permission = new()
								{
									Code = reader[permissionCodeOrdinal].ToStr()
								};

								(requestContext.User.Permissions ??= []).Add(permission);
								requestContext.User.Role ??= new Role(roleId);
							}
						}
						if (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false))
						{
							int permissionCodeOrdinal = reader.GetOrdinal("PermissionCode");
							int urlOrdinal = reader.GetOrdinal("Url");
							while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								UserPermissions permission = new()
								{
									PermissionCode = reader[permissionCodeOrdinal].ToStr(),
									Url = reader[urlOrdinal].ToStr()
								};

								(requestContext.User.UserPermissions ??= []).Add(permission);
							}
						}

						return requestContext;
					}
				}
			}
		}
		catch
		{
			throw;
		}
	}

	public async Task<bool> CreateAPIToken()
	{
		try
		{
			await using EWP_Connection connection = new(ConnectionString);
			await using (connection.ConfigureAwait(false))
			{
				await connection.OpenAsync().ConfigureAwait(false);

				await using EWP_Command command = new("SP_SF_CreatePublicToken", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				await using (command.ConfigureAwait(false))
				{
					command.Parameters.Clear();

					await command.ExecuteNonQueryAsync().ConfigureAwait(false);
					return true;
				}
			}
		}
		catch
		{
			throw;
		}
	}

	public async Task<List<SessionInfo>> GetSessions(User user, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(user);

		List<SessionInfo> returnValue = null;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_Token_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_UserId", user.Id);

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (rdr.HasRows)
						{
							// Cache column ordinals
							int tokenIdOrdinal = rdr.GetOrdinal("TokenId");
							int ipOrdinal = rdr.GetOrdinal("IP");
							int browserOrdinal = rdr.GetOrdinal("Browser");
							int osOrdinal = rdr.GetOrdinal("OS");
							int loginDateOrdinal = rdr.GetOrdinal("LoginDate");
							int locationOrdinal = rdr.GetOrdinal("Location");

							// Check if the "Token" column exists
							bool hasTokenColumn = (await rdr.GetSchemaTableAsync(cancellationToken).ConfigureAwait(false))?.Rows
								.Cast<DataRow>()
								.Select(row => (string)row["ColumnName"])
								.Contains("Token") ?? false;

							int? tokenOrdinal = hasTokenColumn ? rdr.GetOrdinal("Token") : null;

							while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								SessionInfo info = new()
								{
									Hash = rdr[tokenIdOrdinal].ToStr(),
									Address = rdr[ipOrdinal].ToStr(),
									Browser = rdr[browserOrdinal].ToStr(),
									OS = rdr[osOrdinal].ToStr(),
									LoginDate = rdr[loginDateOrdinal].ToDate(),
									Location = rdr[locationOrdinal].ToStr(),
									Token = tokenOrdinal.HasValue ? rdr[tokenOrdinal.Value].ToStr() : null
								};

								(returnValue ??= []).Add(info);
							}
						}
					}
				}
				catch
				{
					throw;
				}
			}
		}

		return returnValue;
	}

	public async Task ToggleMultiSession(string currentHash, bool enableMultiSession, User systemOperator, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(systemOperator);
		ArgumentException.ThrowIfNullOrEmpty(currentHash);

		try
		{
			await using EWP_Connection connection = new(ConnectionString);
			await using (connection.ConfigureAwait(false))
			{
				await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

				await using EWP_Command command = new("SP_SF_User_ToggleMultiSession", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				await using (command.ConfigureAwait(false))
				{
					command.Parameters.Clear();

					_ = command.Parameters.AddWithValue("_Operator", systemOperator?.Id);
					_ = command.Parameters.AddWithValue("_Hash", currentHash);
					_ = command.Parameters.AddWithValue("_Enabled", enableMultiSession);

					_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
				}
			}
		}
		catch
		{
			throw;
		}
	}

	public User ResetPassword(string username)
	{
		User returnValue = null;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_User_Recover", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddCondition("_Code", username, !string.IsNullOrEmpty(username), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User code"));

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				returnValue ??= new User();

				returnValue.DisplayName = rdr["Name"].ToStr();
				returnValue.Id = rdr["id"].ToInt32();
				returnValue.Email = rdr["Email"].ToStr();
				returnValue.LanguageCode = rdr["LanguageValue"].ToStr();
				returnValue.RegisterCode = rdr["registrationCode"].ToStr();
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
		return returnValue;
	}

	// public string ChangePassword(string userHash, PasswordChangeRequest request, User systemOperator)
	// {
	// 	using EWP_Connection connection = new(ConnectionString);
	// 	try
	// 	{
	// 		using EWP_Command command = new("SP_SF_User_UPD", connection)
	// 		{
	// 			CommandType = CommandType.StoredProcedure
	// 		};
	// 		command.Parameters.Clear();

	// 		command.Parameters.AddCondition("_UserHash", userHash, !string.IsNullOrEmpty(userHash), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "UserHash"));
	// 		command.Parameters.AddCondition("_Operator", systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Operator"));
	// 		command.Parameters.AddCondition("_PasswordHash", request.PasswordHash, !string.IsNullOrEmpty(request.PasswordHash), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "PasswordHash"));
	// 		command.Parameters.AddCondition("_TemporalPassword", request.TemporalPassword.Value ? 1 : 0, !(request.TemporalPassword is null));
	// 		_ = command.Parameters.AddWithValue("_Name", null);
	// 		_ = command.Parameters.AddWithValue("_Email", null);
	// 		_ = command.Parameters.AddWithValue("_Status", null);
	// 		_ = command.Parameters.AddWithValue("_LanguageCode", null);
	// 		_ = command.Parameters.AddWithValue("_Code", null);
	// 		_ = command.Parameters.AddWithValue("_RegisterCode", null);
	// 		_ = command.Parameters.AddWithValue("_Notifications", null);
	// 		_ = command.Parameters.AddWithValue("_DefaultLayoutId", null);
	// 		_ = command.Parameters.AddWithValue("_ResetQR", null);
	// 		command.Parameters.AddNull("_AssetCodes");
	// 		command.Parameters.AddNull("_UserTypeCode");
	// 		command.Parameters.AddNull("_PrintingStation");
	// 		command.Parameters.AddNull("_PrintingMachine");
	// 		command.Parameters.AddNull("_EmployeeId");
	// 		command.Parameters.AddNull("_IP");
	// 		command.Parameters.AddNull("_HashDashboards");

	// 		connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	// 		MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	// 		while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
	// 		{
	// 			return rdr["NewHash"].ToStr();
	// 		}
	// 	}
	// 	catch
	// 	{
	// 		throw;
	// 	}
	// 	finally
	// 	{
	// 		connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	// 	}
	// 	return string.Empty;
	// }

	public void ResetQR(User user, User systemOperator)
	{
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_User_UPD", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddCondition("_UserHash", user.Hash, !string.IsNullOrEmpty(user.Hash), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "UserHash"));
			command.Parameters.AddCondition("_Operator", systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Operator"));
			command.Parameters.AddCondition("_PasswordHash", user?.Password, !string.IsNullOrEmpty(user?.Password));
			_ = command.Parameters.AddWithValue("_ResetQR", 1);
			_ = command.Parameters.AddWithValue("_Name", null);
			_ = command.Parameters.AddWithValue("_Email", null);
			_ = command.Parameters.AddWithValue("_Status", null);
			_ = command.Parameters.AddWithValue("_LanguageCode", null);
			_ = command.Parameters.AddWithValue("_Code", null);
			_ = command.Parameters.AddWithValue("_RegisterCode", null);
			_ = command.Parameters.AddWithValue("_Notifications", null);
			_ = command.Parameters.AddWithValue("_DefaultLayoutId", null);
			command.Parameters.AddNull("_AssetCodes");
			command.Parameters.AddNull("_UserTypeCode");
			command.Parameters.AddNull("_PrintingStation");
			command.Parameters.AddNull("_PrintingMachine");
			command.Parameters.AddNull("_EmployeeId");
			command.Parameters.AddNull("_IP");
			command.Parameters.AddNull("_HashDashboards");
			command.Parameters.AddNull("_TemporalPassword");

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			int rdr = command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public async Task<int> GetActiveUserQuantity(CancellationToken cancellationToken = default)
	{
		int returnValue = 0;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_User_Active", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			await using (command.ConfigureAwait(false))
			{
				try
				{
					returnValue = (await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)).ToInt32();
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		return returnValue;
	}

	public MessageBroker UpdateUser(User user, User systemOperator)
	{
		ArgumentNullException.ThrowIfNull(user);
		ArgumentNullException.ThrowIfNull(systemOperator);
		ArgumentException.ThrowIfNullOrEmpty(user.Hash);
		ArgumentException.ThrowIfNullOrEmpty(user.Username);

		MessageBroker returnValue = null;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_User_UPD", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
			_ = command.Parameters.AddWithValue("_UserHash", user.Hash);
			command.Parameters.AddCondition("_PasswordHash", user.Password, !string.IsNullOrEmpty(user.Password));
			command.Parameters.AddCondition("_HashDashboards", user.HashDashboards, !string.IsNullOrEmpty(user.HashDashboards));
			command.Parameters.AddCondition("_Email", user.Email, !string.IsNullOrEmpty(user.Email));
			command.Parameters.AddCondition("_Name", user.DisplayName, !string.IsNullOrEmpty(user.DisplayName));
			command.Parameters.AddCondition("_LanguageCode", user.LanguageCode, !string.IsNullOrEmpty(user.LanguageCode));
			command.Parameters.AddNull("_AssetCodes");
			_ = command.Parameters.AddWithValue("_AssetCodes", user.WorkCenterId);
			command.Parameters.AddNull("_UserTypeCode");
			command.Parameters.AddNull("_PrintingStation");
			command.Parameters.AddNull("_PrintingMachine");
			command.Parameters.AddNull("_EmployeeId");
			command.Parameters.AddNull("_IP");
			if (!string.IsNullOrEmpty(user?.RegisterCode))
			{
				_ = command.Parameters.AddWithValue("_RegisterCode", user.RegisterCode);
				_ = command.Parameters.AddWithValue("_Code", user.Username);
			}
			else
			{
				command.Parameters.AddNull("_RegisterCode");
				command.Parameters.AddNull("_Username");
			}

			command.Parameters.AddCondition("_Status", user.Status.ToInt32(), !string.IsNullOrEmpty(user.RegisterCode));
			_ = command.Parameters.AddWithValue("_DefaultLayoutId", null);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				returnValue = new MessageBroker
				{
					Type = MessageBrokerType.Permission,
					ElementId = null,
					ElementValue = rdr["UserId"].ToStr(),
					MachineId = null,
					Aux = rdr["Aux"].ToStr()
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
		return returnValue;
	}

	public List<User> ListUsers(string userName, User systemOperator, DateTime? DeltaDate = null)
	{
		ArgumentNullException.ThrowIfNull(systemOperator);

		List<User> returnValue = null;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_User_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_Operator", systemOperator?.Id);
			command.Parameters.AddCondition("_Code", userName, !string.IsNullOrEmpty(userName));
			_ = command.Parameters.AddWithValue("_PermissionCode", null);
			command.Parameters.AddNull("_Id");
			command.Parameters.AddNull("_UserHash");
			command.Parameters.AddNull("_DeltaDate");
			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				User element = new()
				{
					Hash = rdr["Hash"].ToStr(),
					DisplayName = rdr["Name"].ToStr(),
					Username = rdr["Code"].ToStr(),
					Email = rdr["Email"].ToStr(),
					SecretKey = rdr["SecretAuth"].ToStr(),
					LanguageCode = rdr["LangCode"].ToStr(),
					Status = (Status)rdr["status"].ToInt32(),
					UserTypeCode = rdr["UserTypeCode"].ToStr(),
					AttendanceControl = rdr["AttendanceControl"].ToBool(),
					ExecutionControl = rdr["ExecutionControl"].ToBool()
				};

				(returnValue ??= []).Add(element);
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
		return returnValue;
	}

	public User GetUser(int userId, User systemOperator)
	{
		ArgumentNullException.ThrowIfNull(systemOperator);

		User returnValue = null;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_User_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_Operator", systemOperator?.Id);
			_ = command.Parameters.AddWithValue("_Id", userId);
			command.Parameters.AddNull("_UserHash");
			command.Parameters.AddNull("_Code");
			command.Parameters.AddNull("_PermissionCode");
			command.Parameters.AddNull("_DeltaDate");
			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				returnValue = new User
				{
					Hash = rdr["Hash"].ToStr(),
					DisplayName = rdr["Name"].ToStr(),
					SecretKey = rdr["SecretAuth"].ToStr(),
					Username = rdr["Code"].ToStr(),
					Email = rdr["Email"].ToStr(),
					LanguageCode = rdr["LangCode"].ToStr(),
					Status = (Status)rdr["status"].ToInt32(),
					DefaultLayout = rdr["DefaultLayoutId"].ToInt32(),
					AttendanceControl = rdr["AttendanceControl"].ToBool(),
					ExecutionControl = rdr["ExecutionControl"].ToBool()
				};
			}
			_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				Permission element = new()
				{
					Code = rdr["PermissionCode"].ToStr()
				};
				(returnValue.Permissions ??= []).Add(element);
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
		return returnValue;
	}

	public User GetUserInfo(string registerCode)
	{
		ArgumentException.ThrowIfNullOrEmpty(registerCode);

		User returnValue = null;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_UserCode_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_RegistrationCode", registerCode);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				returnValue = new User
				{
					Hash = rdr["Hash"].ToStr(),
					DisplayName = rdr["Name"].ToStr(),
					Email = rdr["Email"].ToStr(),
					Status = (Status)rdr["Status"].ToInt32(),
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
		return returnValue;
	}

	public async Task<bool> CheckIPBlock(string IP)
	{
		bool returnValue = false;
		await using EWP_Connection connection = new(ConnectionString);
		try
		{
			await using EWP_Command command = new("SP_SF_IPBlackList_GET", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_IP", IP);

			await connection.OpenAsync().ConfigureAwait(false);
			MySqlDataReader rdr = await command.ExecuteReaderAsync().ConfigureAwait(false);

			while (await rdr.ReadAsync().ConfigureAwait(false))
			{
				returnValue = true;
			}
		}
		catch
		{
			throw;
		}
		finally
		{
			await connection.CloseAsync().ConfigureAwait(false);
		}
		return returnValue;
	}

	public async Task<List<KeyValuePair<string, string>>> GetConfiguration(CancellationToken cancellationToken = default)
	{
		try
		{
			await using EWP_Connection connection = new(ConnectionString);
			await using (connection.ConfigureAwait(false))
			{
				await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

				await using EWP_Command command = new("SP_SF_Settings_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};

				await using (command.ConfigureAwait(false))
				{
					List<KeyValuePair<string, string>> result = [];

					// Execute reader asynchronously
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// Get ordinals for columns
						int keyOrdinal = rdr.GetOrdinal("ConfigKey");
						int valueOrdinal = rdr.GetOrdinal("Value");

						// Read through all the rows
						while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
						{
							string key = rdr[keyOrdinal].ToStr();
							string value = rdr[valueOrdinal].ToStr();

							result.Add(new KeyValuePair<string, string>(key, value));
						}

						return result;
					}
				}
			}
		}
		catch
		{
			throw;
		}
	}

	public void UpdateConfiguration(string key, string value)
	{
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_Settings_UPD", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_Key", key);
			_ = command.Parameters.AddWithValue("_Value", value);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			_ = command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public async Task UpdateConfigurationTab(string key, bool completedTab, bool completedQuestion, string notes)
	{
		await using EWP_Connection connection = new(ConnectionString);
		try
		{
			await using EWP_Command command = new("SP_SF_SettingsTab_UPD", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_Key", key);
			_ = command.Parameters.AddWithValue("_Tab", completedTab);
			_ = command.Parameters.AddWithValue("_Question", completedQuestion);
			_ = command.Parameters.AddWithValue("_Notes", notes);

			await connection.OpenAsync().ConfigureAwait(false);
			await command.ExecuteNonQueryAsync().ConfigureAwait(false);
		}
		catch
		{
			throw;
		}
		finally
		{
			await connection.CloseAsync().ConfigureAwait(false);
		}
	}

	public async Task UpdateConfigurationAnswer(string code, string questionCode, string answer)
	{
		await using EWP_Connection connection = new(ConnectionString);
		try
		{
			await using EWP_Command command = new("SP_SF_SettingsAnswer_UPD", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_Code", code);
			_ = command.Parameters.AddWithValue("_QuestionCode", questionCode);
			_ = command.Parameters.AddWithValue("_Answer", answer);

			await connection.OpenAsync().ConfigureAwait(false);
			await command.ExecuteNonQueryAsync().ConfigureAwait(false);
		}
		catch
		{
			throw;
		}
		finally
		{
			await connection.CloseAsync().ConfigureAwait(false);
		}
	}

	public bool RestoreToken(RequestContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentException.ThrowIfNullOrEmpty(context.Token);

		bool returnValue = false;
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_Token_Restore", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			_ = command.Parameters.AddWithValue("_Token", context.Token);
			_ = command.Parameters.AddWithValue("_UserId", context.User.Id);
			_ = command.Parameters.AddWithValue("_IP", context.IP);
			_ = command.Parameters.AddWithValue("_ExpDate", context.ExpirationDate);
			_ = command.Parameters.AddWithValue("_Browser", context.Browser);
			_ = command.Parameters.AddWithValue("_OS", context.OS);
			_ = command.Parameters.AddWithValue("_Location", context.Location);
			_ = command.Parameters.AddWithValue("_IsPublic", context.IsPublic);
			_ = command.Parameters.AddWithValue("_PublicRoleId", context.User.Role.Id);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			_ = command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			returnValue = true;
		}
		finally
		{
			connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}
		return returnValue;
	}

	// public BoldBIUser GetUserEmployeeData(string registrationCode)
	// {
	// 	ArgumentException.ThrowIfNullOrEmpty(registrationCode);

	// 	BoldBIUser returnValue = null;
	// 	using EWP_Connection connection = new(ConnectionString);
	// 	try
	// 	{
	// 		using EWP_Command command = new("SP_SF_UserEmployee_SEL", connection)
	// 		{
	// 			CommandType = CommandType.StoredProcedure
	// 		};
	// 		command.Parameters.Clear();

	// 		_ = command.Parameters.AddWithValue("_Hash", registrationCode);

	// 		connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	// 		MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	// 		while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
	// 		{
	// 			returnValue = new BoldBIUser
	// 			{
	// 				Email = rdr["Email"].ToStr(),
	// 				FirstName = rdr["Name"].ToStr(),
	// 				Lastname = rdr["Lastname"].ToStr()
	// 			};
	// 		}
	// 	}
	// 	catch
	// 	{
	// 		throw;
	// 	}
	// 	finally
	// 	{
	// 		connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	// 	}
	// 	return returnValue;
	// }
}
