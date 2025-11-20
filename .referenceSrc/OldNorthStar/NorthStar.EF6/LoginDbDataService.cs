using EntityDto.DTO.Admin.Simple;
using EntityDto.LoginDB.DTO;
using Northstar.Core.Identity;
using NorthStar.Core.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.EF6
{
    public class LoginDbDataService
    {
        protected readonly LoginContext _loginContext;
        private string _connectionString;

        public LoginDbDataService(string loginConnectionString)
        {
            _connectionString = loginConnectionString;
            _loginContext = new LoginContext(loginConnectionString);
        }

        public OutputDto_Guid RequestPasswordReset(string userid)
        {
            OutputDto_Guid result = new OutputDto_Guid();
            //UserStoreManager mgr = new UserStoreManager();
            //var result = mgr.SetPassword(userid, password);
            // check and see if this email address is even valid
            var exists = _loginContext.StaffDistricts.FirstOrDefault(p => p.StaffEmail.Equals(userid, System.StringComparison.OrdinalIgnoreCase));
            if (exists == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This email address is not registered in our system. Please contact your district administrator to help you get registered.";
                return result;
            }

            // add record to reset list and send email
            var uid = Guid.NewGuid();
            _loginContext.PasswordResetRequests.Add(new EntityDto.LoginDB.Entity.PasswordResetRequest { ResetRequestDateStamp = DateTime.Now, UserName = userid, UID = uid });
            _loginContext.SaveChanges();

            result.Guid = uid;
            return result;
        }

        public OutputDto_SuccessAndStatus ResetUsersPassword(string userid, string password)
        {
            UserStoreManager mgr = new UserStoreManager(_connectionString);
            var result = mgr.SetPassword(userid, password);

            if(result.IsSuccess)
            {
                return new OutputDto_SuccessAndStatus { isValid = true, Status = new OutputDto_Status { StatusCode = StatusCode.Ok } };
            }
            else
            {
                return new OutputDto_SuccessAndStatus { isValid = false, Status = new OutputDto_Status { StatusCode = StatusCode.UserDisplayableException, StatusMessage = result.Errors.FirstOrDefault() } };
            }
        }

        public OutputDto_SuccessAndStatus ChangeLogin(string username, ClaimsIdentity user)
        {
            UserStoreManager mgr = new UserStoreManager(_connectionString);
            using (System.Data.IDbCommand command = _loginContext.Database.Connection.CreateCommand())
            {
                var oldusername = user.Claims.First(x => x.Type == "preferred_username").Value;
                _loginContext.Database.Connection.Open();
                command.CommandText = "changeUserName";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandTimeout = command.Connection.ConnectionTimeout;
                command.Parameters.Add(new SqlParameter("@newusername", username));
                command.Parameters.Add(new SqlParameter("@oldusername", oldusername));

                command.ExecuteNonQuery();

                return new OutputDto_SuccessAndStatus { isValid = true, Status = new OutputDto_Status { StatusCode = StatusCode.Ok } };
            }
        }

        public OutputDto_SuccessAndStatus ChangePassword(string userid, string password)
        {
            UserStoreManager mgr = new UserStoreManager(_connectionString);
            var result = mgr.SetPassword(userid, password);

            if (result.IsSuccess)
            {
                return new OutputDto_SuccessAndStatus { isValid = true, Status = new OutputDto_Status { StatusCode = StatusCode.Ok } };
            }
            else
            {
                return new OutputDto_SuccessAndStatus { isValid = false, Status = new OutputDto_Status { StatusCode = StatusCode.UserDisplayableException, StatusMessage = result.Errors.FirstOrDefault() } };
            }
        }

        public OutputDto_SuccessAndStatus ResetUsersPasswordFromEmail(Guid uid, string password)
        {
            var username = _loginContext.PasswordResetRequests.First(p => p.UID == uid).UserName;

            UserStoreManager mgr = new UserStoreManager(_connectionString);

            var result = mgr.SetPassword(username, password);

            if (result.IsSuccess)
            {
                // clean up passworrequests
                var requestsToRemove = _loginContext.PasswordResetRequests.Where(p => p.UserName == username);
                _loginContext.PasswordResetRequests.RemoveRange(requestsToRemove);
                _loginContext.SaveChanges();
                return new OutputDto_SuccessAndStatus { isValid = true, Status = new OutputDto_Status { StatusCode = StatusCode.Ok } };
            }
            else
            {
                return new OutputDto_SuccessAndStatus { isValid = false, Status = new OutputDto_Status { StatusCode = StatusCode.UserDisplayableException, StatusMessage = result.Errors.FirstOrDefault() } };
            }
        }

        public OutputDto_SuccessAndStatus ValidateUID(InputDto_PasswordReset input)
        {
            OutputDto_SuccessAndStatus result = new OutputDto_SuccessAndStatus();
            //UserStoreManager mgr = new UserStoreManager();
            //var result = mgr.SetPassword(userid, password);
            // check and see if this email address is even valid
            var dateLimit = DateTime.Now.AddHours(-24);
            var exists = _loginContext.PasswordResetRequests.FirstOrDefault(p => p.UID == input.UID && p.ResetRequestDateStamp > dateLimit);
            if (exists == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Password Reset link has expired or is invalid.";
                return result;
            }

            // add record to reset list and send email

            return result;
        }
    }
}
