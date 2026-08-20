namespace EWP.SF.Common.Models;

/// <summary>
/// Represents a class that contains permission constants for various functionalities in the system.
/// </summary>
public static class Permissions
{
	#region System

	/// <summary>
	/// Permission to manage email settings.
	/// </summary>
	public const string SYSTEM_EMAIL_MANAGEMENT = "697EFC4D308C438F896B0FC2FAB50246";

	#endregion System

	#region Users

	/// <summary>
	/// Permission to view the list of users.
	/// </summary>
	public const string USERS_LIST = "19F5908E62458EE4F483834565FA28D5";

	/// <summary>
	/// Permission to edit a user.
	/// </summary>
	public const string USERS_EDIT = "D7F50CB704FD47CA1CAC632CE40F036B";

	/// <summary>
	/// Permission to create a new user.
	/// </summary>
	public const string USERS_CREATE = "7D2338B5F73521F7F350148BCBC93A95";

	/// <summary>
	/// Permission to delete a user.
	/// </summary>
	public const string USERS_DELETE = "40F28EACBD80CDA409E03607342941A5";

	#endregion Users

	#region Security

	/// <summary>
	/// Permission to manage user groups.
	/// </summary>
	public const string ROLES_ADD = "76720096E14B14DF5A2E193CDB57D27C";

	/// <summary>
	/// Permission to edit user groups.
	/// </summary>
	public const string ROLES_EDIT = "32758DAAE1ABE0ABEC0DC2045AB127B3";

	/// <summary>
	/// Permission to delete user groups.
	/// </summary>
	public const string ROLES_DELETE = "1C3DB04D1C243A351C6E3D3D92BDD134";

	/// <summary>
	/// Permission to manage user groups.
	/// </summary>
	public const string RECEIVE_FEEDBACK_MESSAGES = "A9AB557E93EEB40CDF85316AF0B8B4D9";

	#endregion Security

	#region Machines and Sensors

	/// <summary>
	/// Permission to create a new machine.
	/// </summary>
	public const string CP_MACHINE_CREATE = "AA1D014F4CDAA8D113A15B2C0E40C0A8";

	/// <summary>
	/// Permission to edit a machine.
	/// </summary>
	public const string CP_MACHINE_EDIT = "8D0314E8270437A5BA16C3896BEDA27E";

	/// <summary>
	/// Permission to delete a machine.
	/// </summary>
	public const string CP_MACHINE_DELETE = "5C429BC7CB55E8A4ECC15B4ADD175895";

	/// <summary>
	/// Permission to manage a machine.
	/// </summary>
	public const string CP_MACHINE_MANAGE = "C08A6149381BADE65E4310876E17AC66";

	/// <summary>
	/// Permission to view a machine.
	/// </summary>
	public const string CP_SENSOR_EDIT = "6EFB54CB7FE75C086ACA8D9A39671DAC";

	/// <summary>
	/// Permission to create a new sensor.
	/// </summary>
	public const string CP_SENSOR_DELETE = "B8B55ECFA632282AEE55507323036D18";

	/// <summary>
	/// Permission to edit a sensor.
	/// </summary>
	public const string CP_SENSOR_CREATE = "C43518B4BDDF9FB3E97B3EDCED011C53";

	/// <summary>
	/// Permission to manager a sensor.
	/// </summary>
	public const string CP_SENSOR_MANAGE = "AF29AA178AE43211319E28F627E96590";

	/// <summary>
	/// Permission to input data into a machine.
	/// </summary>
	public const string CP_MACHINE_INPUT = "9A59DE78E863CAF15514DFC10CC6A239";

	/// <summary>
	/// Permission to manage application settings.
	/// </summary>
	public const string CP_MGR_APPSETTINGS = "7C65248FD8F55051F6B79DA9CC5EEF94";

	#endregion Machines and Sensors

	#region Production Lines

	/// <summary>
	/// Permission to manage production lines.
	/// </summary>
	public const string CP_PRODUCTIONLINE_MANAGE = "0A3BD6D185BEADED4AEFFD9B0CDCF0E8";

	/// <summary>
	/// Permission to create a new production line.
	/// </summary>
	public const string CP_PRODUCTIONLINE_CREATE = "B86F66BCAEE75D14AEB3FE4614491294";

	/// <summary>
	/// Permission to edit a production line.
	/// </summary>
	public const string CP_PRODUCTIONLINE_DELETE = "98557D44BF7253308CE401247F50DDD4";

	#endregion Production Lines

	#region Catalogs

	/// <summary>
	/// Permission to manage catalogs.
	/// </summary>
	public const string CAT_CATALOGS_MANAGE = "92859451E3843E4AF3DC83D712147078";

	/// <summary>
	/// Permission to manage languages.
	/// </summary>
	public const string CAT_LANGUAGE_MANAGE = "3860316C99826AA0E56546A56B268B4A";

	/// <summary>
	/// Permission to manage products.
	/// </summary>
	public const string CAT_ROLES_MANAGE = "822795E6F1E29E4CCE845A99112E58ED";

	#endregion Catalogs

	#region Reports

	/// <summary>
	/// Permission to view reports.
	/// </summary>
	public const string REPORTS_LIST = "F426990405FFA70B0F388E68A233BB3D";

	#endregion Reports

	#region Backup History

	/// <summary>
	/// Permission to view backup history.
	/// </summary>
	public const string CP_USER_LOG_VIEW = "17252B3945F6F96158BAA1AA88352C6B";

	#endregion Backup History

	#region Manual downtime

	/// <summary>
	/// Permission to manage manual downtime.
	/// </summary>
	public const string PRD_MANUALDOWNTIME_MANAGE = "2728BB6BA9DB5FA0BD06F9971DBC79D9";

	#endregion Manual downtime

	#region Measure Unit

	/// <summary>
	/// Permission to manage measure units.
	/// </summary>
	public const string PRD_MEASUREUNIT_MANAGE = "43F499052F0FAE0666C375F74215FFC4";

	#endregion Measure Unit

	#region Process entry

	/// <summary>
	/// Permission to manage process entries.
	/// </summary>
	public const string PRD_PROCESS_ENTRY_MANAGE = "E831235429B59F9724F1643440106997";

	/// <summary>
	/// Permission to manage order progress.
	/// </summary>
	public const string PRD_ORDERPROGRESS_MANAGE = "3AA42354B92485EA4DD1D9C8AEB2AFE8";

	#endregion Process entry

	#region Human Resources

	/// <summary>
	/// Permission to manage employee records.
	/// </summary>
	public const string HR_EMPLOYEE_MANAGE = "F72DB49F30170CF7574D1974D7BBC333";

	#endregion Human Resources

	#region Tool Type

	/// <summary>
	/// Permission to manage tool types.
	/// </summary>
	public const string PRD_TOOLTYPE_MANAGE = "4ED4AF42801E73064B0EAA8661EDF6FA";

	#endregion Tool Type

	#region Tool

	/// <summary>
	/// Permission to manage tools.
	/// </summary>
	public const string PRD_TOOL_MANAGE = "DD70DCB55013EC4F6B0B4FF8D0BA8E2C";

	#endregion Tool

	#region Tool

	/// <summary>
	/// Permission to manage tool entries.
	/// </summary>
	public const string PRD_WORKORDER_MANAGE = "66858919588F38C5FD8CAFCF22D2FE9A";

	#endregion Tool

	#region Assets Facility

	/// <summary>
	/// Permission to manage assets facility.
	/// </summary>
	public const string ASS_FACILITY_MANAGE = "210687DD6F09777FA6D771AC4EF43D9E";

	#endregion Assets Facility

	#region Assets Floor

	/// <summary>
	/// Permission to manage assets floor.
	/// </summary>
	public const string ASS_FLOOR_MANAGE = "C468501A3F240D0552E44E37165B86EF";

	#endregion Assets Floor

	#region Assets Work Center

	/// <summary>
	/// Permission to manage assets work center.
	/// </summary>
	public const string ASS_WORKCENTER_MANAGE = "415CFBFC61814F7804A4181DFA31A5CE";

	#endregion Assets Work Center

	#region Master Data

	/// <summary>
	/// Permission to manage master data.
	/// </summary>
	public const string MSRD_PRODUCTION_ATTRIBUTE_MANAGE = "F0556FA418535D2B249FBCD2CF78F32C";

	#endregion Master Data

	#region Process Type

	/// <summary>
	/// Permission to manage process types.
	/// </summary>
	public const string CP_PROCESS_TYPE_MANAGE = "3D727B3EE9DAEFBF58392DC5D337CFD1";

	#endregion Process Type

	#region Connectivity

	/// <summary>
	/// Permission to manage API tokens.
	/// </summary>
	public const string CN_APITOKENS_MANAGE = "95B08799E7019A7BD5E3E999AD699E7C";

	#endregion Connectivity

	/// <summary>
	/// Permission to execute query manager.
	/// </summary>
	public const string CN_QUERYMANAGER_EXEC = "030CCDD90F2011EC856900155D6F4317";

	/// <summary>
	/// Permission to view machine details.
	/// </summary>
	public const string RPT_MACHINEDETAILS_VW = "20294150232130097A2EF73C8218C3F6";

	#region Production Scheduling

	/// <summary>
	/// Permission to view production scheduling.
	/// </summary>
	public const string CP_PRODUCTION_SCHEDULING_VW = "DDFDC2A2B4C277158589A9F95FA2FBD8";

	/// <summary>
	/// Permission to manage shift status.
	/// </summary>
	public const string CP_SCHEDULING_SHIFT_STATUS_MANAGE = "D28E9C53075904BABFD36F7B7EA33F02";

	/// <summary>
	/// Permission to manage work orders.
	/// </summary>
	public const string CP_SCHEDULING_SHIFT_MANAGE = "D9309726FB3B0CA4F787ACE4C6E341DE";

	#endregion Production Scheduling

	#region Inventory

	/// <summary>
	/// Permission to manage warehouse inventory.
	/// </summary>
	public const string INV_WAREHOUSE_MANAGE = "BAEFDC4C9C1553F5840D03A8D578C6D9";

	/// <summary>
	/// Permission to manage inventory.
	/// </summary>
	public const string INV_INVENTORY_MANAGE = "0BBDCDDF7D9DB3322A48C41EE76F20ED";

	#endregion Inventory

	#region SalesOrder

	/// <summary>
	/// Permission to manage sales orders.
	/// </summary>
	public const string INV_SALESORDER_LST = "D9B5AAB07B3B540F791BA58667A18380";

	#endregion SalesOrder

	#region PurchaseOrder

	/// <summary>
	/// Permission to manage purchase orders.
	/// </summary>
	public const string INV_PURCHASEORDER_LST = "56EA0EE77C2C6EFE0131DFEEA01705F5";

	#endregion PurchaseOrder

	#region Notifications

	/// <summary>
	/// Permission to manage notifications.
	/// </summary>
	public const string CP_NOTIFICATIONS_PLATFORMS = "D7C40E96F7EC957CE309630538DF24F3";

	/// <summary>
	/// Permission to manage notification details.
	/// </summary>
	public const string CP_NOTIFICATIONS_PLATFORMSDETAIL = "1F12930AE41D1F4DDDF551F713434FE6";

	/// <summary>
	/// Permission to manage notification templates.
	/// </summary>
	public const string CP_NOTIFICATIONS_GROUP = "ABC646B6ABD2333E0F79345AAD2CEABE";

	/// <summary>
	/// Permission to manage notification template details.
	/// </summary>
	public const string CP_NOTIFICATIONS_TEMPLATES = "5334591454D2589487C3AC194249B3D7";

	#endregion Notifications

	#region Bold BI

	/// <summary>
	/// Permission to view Bold BI dashboards.
	/// </summary>
	public const string BOLDBI_DASHBOARD_VIEW = "40242798340912403DAC979F64F8FDA8";

	#endregion Bold BI

	/// <summary>
	/// Permission to manage NLog settings.
	/// </summary>
	public const string NLOG_MANAGE = "B2D37AE1CEDF42FF874289B721860AF2";

	#region DataSyncManager

	/// <summary>
	/// Permission to manage data synchronization.
	/// </summary>
	public const string DATA_SYNC_MANAGER = "C47DF1EA1850E954BDF5CC63BE0CB89E";

	#endregion DataSyncManager

	#region Scheduling SecondaryConstraintsGroup

	/// <summary>
	/// Permission to view secondary constraint groups.
	/// </summary>
	public const string SP_SECONDARY_CONTAINST_GROUP_MANAGE = "34831ABBFA33D6F99BA9E5D9713FB15B";

	#endregion Scheduling SecondaryConstraintsGroup
}
