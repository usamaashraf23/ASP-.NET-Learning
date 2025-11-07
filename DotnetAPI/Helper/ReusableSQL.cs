using Dapper;
using DotnetAPI.Data1;
using DotnetAPI.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

namespace DotnetAPI.Helper
{
    public class ReusableSQL
    {
        private readonly DataContextDapper _dapper;
        public ReusableSQL(IConfiguration config)
        {

            _dapper = new DataContextDapper(config);
        }

        public bool UpsertUser(UserComplete user)
        {
            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@FirstNameParam", user.FirstName, DbType.String);
            sqlParameters.Add("@LastNameParam", user.LastName, DbType.String);
            sqlParameters.Add("@EmailParam", user.Email, DbType.String);
            sqlParameters.Add("@GenderParam", user.Gender, DbType.String);
            sqlParameters.Add("@ActiveParam", user.Active, DbType.Boolean);
            sqlParameters.Add("@SalaryParam", user.Salary, DbType.Double);
            sqlParameters.Add("@DepartmentParam", user.Department, DbType.String);
            sqlParameters.Add("@JobTitleParam", user.JobTitle, DbType.String);
            sqlParameters.Add("@UserIdParam", user.UserId, DbType.Int16);

            string sql = $@"EXEC TutorialAppSchema.spUser_Upsert
                         @FirstName = @FirstNameParam, 
                         @LastName = @LastNameParam, 
                         @Email = @EmailParam, 
                         @Gender = @GenderParam, 
                         @Active = @ActiveParam,
                         @Salary = @SalaryParam,
                         @Department = @DepartmentParam,
                         @JobTitle = @JobTitleParam,
                         @UserId = @UserIdParam";

            return _dapper.ExecuteWithParameters(sql, sqlParameters);
        }
    }
}
