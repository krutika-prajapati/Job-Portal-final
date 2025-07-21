using Microsoft.EntityFrameworkCore;
using Repository.Enum;
using Repository.Interface;
using Repository.Model;
using Shared.DTO;

namespace Repository.Repository
{
    public class JobRepo:IJobRepo
    {
        private readonly DBContext context;

        public JobRepo(DBContext context)
        {
            this.context = context;
        }

        public async Task<bool> CreateJob(Job job)
        {
            try
            {
                using (context)
                {
                    await context.Jobs.AddAsync(job);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<JobCretedByUser>> GetJobsCreateByUser(int userId)
        {
            try
            {
                var company = await context.Companies
                    .Where(c => c.UserId == userId && c.Active)
                    .FirstOrDefaultAsync();

                if (company == null)
                    return new List<JobCretedByUser>();

                int companyId = company.CompanyId;

                return await context.Jobs
                    .Where(job => job.CompanyId == companyId && job.Active)
                    .Select(job => new JobCretedByUser
                    {
                        JobTitle = job.Title,
                        JobDescription = job.Description,
                        JobLocation = job.Location,
                        SalaryRange = job.SalaryRange,
                        JobType = job.JobType.ToString(),
                        CompanyName = company.Name,
                        CompanyDescription= company.Description,
                        CompanyWebsite=company.Website,
                        PostedDate = DateOnly.FromDateTime(job.PostedDate)
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<JobCretedByUser>();
            }
        }

        public async Task<List<JobCretedByUser>> GetAllJobs()
        {
            try
            {

                return await (
                    from job in context.Jobs
                    join company in context.Companies
                    on job.CompanyId equals company.CompanyId
                    where job.Active && company.Active
                    select new JobCretedByUser
                    {
                        JobTitle = job.Title,
                        JobDescription = job.Description,
                        JobLocation = job.Location,
                        SalaryRange = job.SalaryRange,
                        JobType = job.JobType.ToString(),
                        CompanyName = company.Name,
                        CompanyDescription = company.Description,
                        CompanyWebsite = company.Website,
                        PostedDate = DateOnly.FromDateTime(job.PostedDate)
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<JobCretedByUser>();
            }
        }

        public async Task<bool> DeleteJobById(int jobId, int userId)
        {
            try
            {
                // Get user and role name using a join
                var userWithRole = await (
                    from user in context.Users
                    join role in context.UserRoles on user.RoleId equals role.RoleId
                    where user.UserId == userId && user.Active
                    select new { user, RoleName = role.Name }
                ).FirstOrDefaultAsync();

                if (userWithRole == null)
                    return false;

                // Get job and its company (with company owner) using a join
                var jobWithCompany = await (
                    from job in context.Jobs
                    join company in context.Companies on job.CompanyId equals company.CompanyId
                    where job.JobId == jobId && job.Active
                    select new { job, CompanyOwnerId = company.UserId }
                ).FirstOrDefaultAsync();

                if (jobWithCompany == null)
                    return false;

                // Check if user is Admin or company owner
                bool isAdmin = userWithRole.RoleName == Role.Admin.ToString();
                bool isCompanyOwner = jobWithCompany.CompanyOwnerId == userId;

                if (!isAdmin && !isCompanyOwner)
                    return false;

                // Soft delete: set Active = false
                jobWithCompany.job.Active = false;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateJob(int userId, UpdateJob updatedJob)
        {
            try
            {
                // Get user and role name using a join
                var userWithRole = await (
                    from user in context.Users
                    join role in context.UserRoles on user.RoleId equals role.RoleId
                    where user.UserId == userId && user.Active
                    select new { user, RoleName = role.Name }
                ).FirstOrDefaultAsync();

                if (userWithRole == null)
                    return false;

                // Get job and its company (with company owner) using a join
                var jobWithCompany = await (
                    from job in context.Jobs
                    join company in context.Companies on job.CompanyId equals company.CompanyId
                    where job.JobId == updatedJob.JobId && job.Active
                    select new { job, CompanyOwnerId = company.UserId }
                ).FirstOrDefaultAsync();

                if (jobWithCompany == null)
                    return false;

                // Check if user is Admin or company owner
                bool isAdmin = userWithRole.RoleName == Role.Admin.ToString();
                bool isCompanyOwner = jobWithCompany.CompanyOwnerId == userId;

                if (!isAdmin && !isCompanyOwner)
                    return false;

                // Update job fields
                jobWithCompany.job.Title = updatedJob.Title;
                jobWithCompany.job.Description = updatedJob.Description;
                jobWithCompany.job.Location = updatedJob.Location;
                jobWithCompany.job.SalaryRange = updatedJob.SalaryRange;

                if (System.Enum.TryParse<JobType>(updatedJob.JobType, true, out var parsedJobType))
                {
                    jobWithCompany.job.JobType = parsedJobType;
                }
                else
                {
                    return false;
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
