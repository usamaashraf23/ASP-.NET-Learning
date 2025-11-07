using Dapper;
using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController:ControllerBase
{
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;
    public PostController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _config = config;
    }

    [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
    public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
    {
        DynamicParameters sqlParameters = new DynamicParameters();
        
        string sql = $@"EXEC TutorialAppSchema.spPosts_Get";

        string parameters = "";

        if(userId != 0)
        {
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            parameters += $", @UserId=@UserIdParameter";
        }
        if(searchParam != "None")
        {
            sqlParameters.Add("@SearchParam", searchParam, DbType.String);
            parameters += $", @SearchValue=@SearchParam";
        }
        if (postId != 0)
        {
            sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);
            parameters += $", @PostId=@PostIdParameter";
        }

        if(parameters.Length > 0)
        {
            sql += parameters.Substring(1);
        }

        Console.WriteLine(sql);

        IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);

        return posts;
    }


    [HttpGet("GetMyPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
        int userId = int.Parse(this.User.FindFirst("userId")?.Value);

        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        string sql = $@"EXEC TutorialAppSchema.spPosts_Get
                        @UserId = @UserIdParameter";

        IEnumerable<Post> myPosts = _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);

        return myPosts;
    }

    [HttpPut("UpsertPost")]
    public IActionResult UpsertPost(Post post)
    {
        int userId = int.Parse(this.User.FindFirst("userId")?.Value);
        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@PostTitleParameter", post.PostTitle, DbType.String);
        sqlParameters.Add("@PostDescriptionParameter", post.PostDescription, DbType.String);
        sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        string sql = $@"EXEC TutorialAppSchema.spPost_Upsert
	                    @UserId = @UserIdParameter,
	                    @PostTitle = @PostTitleParameter,
	                    @PostDescription = @PostDescriptionParameter";



        if (post.PostId > 0)
        {
            sqlParameters.Add("@PostIdParam", post.PostId, DbType.Int32);
            sql += $", @PostId = @PostIdParam";
        }

        if(_dapper.ExecuteWithParameters(sql, sqlParameters))
        {
            return Ok();
        }
        throw new Exception("Failed to add new post");
    }


    [HttpDelete("DeletePost/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@PostIdParam", postId, DbType.Int32);
        sqlParameters.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
        string sql = $@"EXEC TutorialAppSchema.spPost_Delete
                        @PostId = @PostIdParam,
                        @UserId = @UserIdParam";

        if (_dapper.ExecuteWithParameters(sql, sqlParameters))
        {
            return Ok();
        }
        throw new Exception("Failed to delete post");
    }

    [HttpGet("SearchPost/{searchParam}")]
    public IEnumerable<Post> SearchPost(string searchParam)
    {
        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@SearchParam", searchParam, DbType.String);
        string sql = $@"SELECT [PostId],
                            [UserId],
                            [PostTitle],
                            [PostDescription],
                            [PostCreated],
                            [PostUpdated] FROM TutorialAppSchema.Posts
                            WHERE PostTitle LIKE +'%'+ @SearchParam +'%' OR PostDescription LIKE '%'+ @SearchParam +'%'";

        IEnumerable<Post> posts = _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);

        return posts;
    }

}
