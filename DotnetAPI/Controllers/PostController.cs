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
    public IEnumerable<Post> GetPosts(int postId, int userId, string searchParam)
    {
        string sql = $@"EXEC TutorialAppSchema.spPosts_Get
                        @PostId = {postId},
                        @UserId = {userId},
                        @SearchValue = '{searchParam}'";

        IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);

        return posts;
    }

    [HttpGet("GetSinglePost/{postId}")]
    public Post GetSinglePost(int postId)
    {
        string sql = $@"SELECT [PostId],
                        [UserId],
                        [PostTitle],
                        [PostDescription],
                        [PostCreated],
                        [PostUpdated] FROM TutorialAppSchema.Posts
                        WHERE PostId = {postId}";

        Post post = _dapper.LoadDataSingle<Post>(sql);

        return post;
    }

    [HttpGet("GetMyPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
        int userId = int.Parse(this.User.FindFirst("userId")?.Value);
        string sql = $@"SELECT [PostId],
                        [UserId],
                        [PostTitle],
                        [PostDescription],
                        [PostCreated],
                        [PostUpdated] FROM TutorialAppSchema.Posts
                        WHERE UserId = {userId}";

        IEnumerable<Post> myPosts = _dapper.LoadData<Post>(sql);

        return myPosts;
    }

    [HttpPost("AddPost")]
    public IActionResult AddPost(PostToAddDTO post)
    {
        int userId = int.Parse(this.User.FindFirst("userId")?.Value);
        string sql = $@"INSERT INTO TutorialAppSchema.Posts(
                        [UserId],
                        [PostTitle],
                        [PostDescription],
                        [PostCreated],
                        [PostUpdated]
                        ) VALUES(
                            {userId}
                            ,'{post.PostTitle}'
                            ,'{post.PostDescription}'
                            ,GETDATE(),
                            GETDATE()
                        )";

        if(_dapper.Execute(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to add new post");
    }

    [HttpPut("EditPost")]

    public IActionResult EditPost(PostToUpdateDTO post)
    {
        string sql = $@"UPDATE TutorialAppSchema.Posts
                        SET [PostTitle] = '{post.PostTitle}',
                            [PostDescription] = '{post.PostDescription}',
                            [PostUpdated] = GETDATE()
                        WHERE PostId = {post.PostId}";
        if (_dapper.Execute(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to Update Post");
    }

    [HttpDelete("DeletePost/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        string sql = $@"DELETE FROM TutorialAppSchema.Posts
                        WHERE PostId = {postId} AND UserId = {this.User.FindFirst("userId")?.Value}";

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
