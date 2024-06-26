using System.Data;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Helpers;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DotNetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]

public class UserController : ControllerBase
{
    private readonly DataContextDapper _dapper;

    private readonly ReusableSql _reusableSql;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _reusableSql = new ReusableSql(config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = "EXEC TutorialAppSchema.spUsers_Get";
        string stringParameters = "";
        DynamicParameters sqlParameters = new DynamicParameters();

        if (userId != 0)
        {
            stringParameters += ", @UserId= @UserIdParameter";
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        }
        if (isActive)
        {
            stringParameters += ", @Active= @isActiveParameter";
            sqlParameters.Add("@isActiveParameter", isActive, DbType.Boolean);
        }

        if (stringParameters.Length > 0) sql += stringParameters.Substring(1);
        IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql, sqlParameters);
        return users;
    }

    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        if (_reusableSql.UpsertUser(user)) return Ok();
        throw new Exception("Failed to Upsert User");
    }


    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = "EXEC TutorialAppSchema.spUsers_Delete @UserId = @UserIdParameter";

        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);

        if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters)) return Ok();
        throw new Exception("Failed to Delete User");
    }

}