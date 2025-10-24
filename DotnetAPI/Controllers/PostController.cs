using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        string sql = $@"EXEC TutorialAppSchema.spPosts_Get";

        string parameters = "";

        if(userId != 0)
        {
            parameters += $", @UserId={userId}";
        }
        if(searchParam != "None")
        {
            parameters += $", @SearchValue={searchParam}";
        }
        if (postId != 0)
        {
            parameters += $", @PostId={postId}";
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
        string sql = $@"EXEC TutorialAppSchema.spPosts_Get
                        @UserId = {userId}";

        IEnumerable<Post> myPosts = _dapper.LoadData<Post>(sql);

        return myPosts;
    }

    [HttpPut("UpsertPost")]
    public IActionResult UpsertPost(Post post)
    {
        int userId = int.Parse(this.User.FindFirst("userId")?.Value);
        string sql = $@"EXEC TutorialAppSchema.spPost_Upsert
	                    @UserId = {userId},
	                    @PostTitle = '{post.PostTitle}',
	                    @PostDescription = '{post.PostDescription}'";

        Console.WriteLine(sql);

        if (post.PostId > 0)
        {
            sql += $", @PostId = {post.PostId}";
        }

        Console.WriteLine(sql);

        if(_dapper.Execute(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to add new post");
    }


    [HttpDelete("DeletePost/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        string sql = $@"EXEC TutorialAppSchema.spPost_Delete
                        @PostId = {postId},
                        UserId = {this.User.FindFirst("userId")?.Value} ";

        if (_dapper.Execute(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to delete post");
    }

    [HttpGet("SearchPost/{searchParam}")]
    public IEnumerable<Post> SearchPost(string searchParam)
    {
        string sql = $@"SELECT [PostId],
                            [UserId],
                            [PostTitle],
                            [PostDescription],
                            [PostCreated],
                            [PostUpdated] FROM TutorialAppSchema.Posts
                            WHERE PostTitle LIKE '%{searchParam}%' OR PostDescription LIKE '%{searchParam}%'";

        IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);

        return posts;
    }

}
