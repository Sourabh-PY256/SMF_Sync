using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessLayer;
using EWP.SF.KafkaSync.DataAccess;
using Moq;

namespace EWP.SF.KafkaSync.Tests;

public class ClockInOutOperationTests
{
    private readonly Mock<IClockInOutRepo> _clockInOutRepo = new();
    private readonly Mock<IEmployeeRepo> _employeeRepo = new();
    private readonly User _systemOperator = new() { Id = 1, EmployeeId = "EMP-1" };

    private ClockInOutOperation CreateSut() => new(_clockInOutRepo.Object, _employeeRepo.Object);

    [Fact]
    public void ListUpdateCLockInOutBulk_EmptyEmployeeCode_ReturnsFailedResponseAndMergesEmptyList()
    {
        // Arrange
        _employeeRepo.Setup(x => x.ListEmployees(string.Empty, string.Empty, null)).Returns([]);
        var sut = CreateSut();
        var records = new List<ClockInOutDetailsExternal>
        {
            new() { ClockInOutId = "CIO-1", EmployeeCode = "", StartDate = DateTime.UtcNow }
        };

        // Act
        var result = sut.ListUpdateCLockInOutBulk(records, records, _systemOperator, false, LevelMessage.Success);

        // Assert
        Assert.False(result[0].IsSuccess);
        Assert.Contains("Employee code is required", result[0].Message);
        // Matches the 2503 source behavior: the merge call still fires (with an empty "[]" payload)
        // because it's gated on the original input count, not on how many records passed validation.
        _clockInOutRepo.Verify(x => x.MergeClockInOutBulk("[]", _systemOperator, false), Times.Once);
    }

    [Fact]
    public void ListUpdateCLockInOutBulk_UnknownEmployeeCode_ReturnsDoesNotExistError()
    {
        // Arrange
        _employeeRepo.Setup(x => x.ListEmployees(string.Empty, string.Empty, null)).Returns([]);
        var sut = CreateSut();
        var records = new List<ClockInOutDetailsExternal>
        {
            new() { ClockInOutId = "CIO-1", EmployeeCode = "GHOST-1", StartDate = DateTime.UtcNow }
        };

        // Act
        var result = sut.ListUpdateCLockInOutBulk(records, records, _systemOperator, false, LevelMessage.Success);

        // Assert
        Assert.False(result[0].IsSuccess);
        Assert.Contains("does not exist", result[0].Message);
    }

    [Fact]
    public void ListUpdateCLockInOutBulk_MissingStartDate_ReturnsStartDateRequiredError()
    {
        // Arrange
        var employees = new List<Employee> { new() { Code = "E1", ExternalId = "ERP-E1" } };
        _employeeRepo.Setup(x => x.ListEmployees(string.Empty, string.Empty, null)).Returns(employees);
        var sut = CreateSut();
        var records = new List<ClockInOutDetailsExternal>
        {
            new() { ClockInOutId = "CIO-1", EmployeeCode = "ERP-E1", StartDate = null }
        };

        // Act
        var result = sut.ListUpdateCLockInOutBulk(records, records, _systemOperator, false, LevelMessage.Success);

        // Assert
        Assert.False(result[0].IsSuccess);
        Assert.Contains("Start Date is required", result[0].Message);
    }

    [Fact]
    public void ListUpdateCLockInOutBulk_ValidRecord_ResolvesInternalCodeAndMergesBulk()
    {
        // Arrange - ERP sends the external employee code; the operation must resolve it to the
        // internal SF employee code before merging, matching the 2503 monolith's behavior.
        var employees = new List<Employee> { new() { Code = "E1", ExternalId = "ERP-E1" } };
        _employeeRepo.Setup(x => x.ListEmployees(string.Empty, string.Empty, null)).Returns(employees);
        string? mergedJson = null;
        _clockInOutRepo
            .Setup(x => x.MergeClockInOutBulk(It.IsAny<string>(), _systemOperator, false))
            .Callback<string, User, bool>((json, _, _) => mergedJson = json);
        var sut = CreateSut();
        var records = new List<ClockInOutDetailsExternal>
        {
            new() { ClockInOutId = "CIO-1", EmployeeCode = "ERP-E1", StartDate = DateTime.UtcNow }
        };

        // Act
        var result = sut.ListUpdateCLockInOutBulk(records, records, _systemOperator, false, LevelMessage.Success);

        // Assert
        Assert.True(result[0].IsSuccess);
        Assert.Equal("CIO-1", result[0].Code);
        _clockInOutRepo.Verify(x => x.MergeClockInOutBulk(It.IsAny<string>(), _systemOperator, false), Times.Once);
        Assert.NotNull(mergedJson);
        Assert.Contains("\"E1\"", mergedJson); // Merged payload should carry the resolved internal code, not the ERP one.
    }

    [Fact]
    public void ListUpdateCLockInOutBulk_ErrorLevelFilter_OnlyReturnsFailedRecords()
    {
        // Arrange
        var employees = new List<Employee> { new() { Code = "E1", ExternalId = "ERP-E1" } };
        _employeeRepo.Setup(x => x.ListEmployees(string.Empty, string.Empty, null)).Returns(employees);
        var sut = CreateSut();
        var records = new List<ClockInOutDetailsExternal>
        {
            new() { ClockInOutId = "CIO-1", EmployeeCode = "ERP-E1", StartDate = DateTime.UtcNow }, // succeeds
            new() { ClockInOutId = "CIO-2", EmployeeCode = "", StartDate = DateTime.UtcNow } // fails
        };

        // Act
        var result = sut.ListUpdateCLockInOutBulk(records, records, _systemOperator, false, LevelMessage.Error);

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsSuccess);
    }
}
