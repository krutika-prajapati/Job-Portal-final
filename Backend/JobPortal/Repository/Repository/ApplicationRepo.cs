using Microsoft.EntityFrameworkCore;
using Repository.Enum;
using Repository.Interface;
using Repository.Model;
using Shared.DTO;

namespace Repository.Repository
{
    public class ApplicationRepo:
        IApplicationRepo
    {
        private readonly DBContext context;

        public ApplicationRepo(DBContext context)
        {
            this.context = context;
        }

        public async Task<bool> CreateApplication(Application application)
        {
            try
            {
                using (context)
                {
                    await context.Applications.AddAsync(application);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;

            }
        }
        public async Task<bool> ApplicationExists(int userId, int jobId)
        {
            try
            {
                return await context.Applications
                    .AnyAsync(a => a.UserId == userId && a.JobId == jobId && a.Active);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<ApplicationDetails>> GetApplicationsByUserId(int userId)
        {
            try
            {
                var applications = await (from application in context.Applications
                                          join job in context.Jobs on application.JobId equals job.JobId
                                          join company in context.Companies on job.CompanyId equals company.CompanyId
                                          where application.UserId == userId && application.Active
                                          select new ApplicationDetails
                                          {
                                              ApplicationId = application.ApplicationId,
                                              AppliedDate = DateOnly.FromDateTime(application.AppliedDate),
                                              ApplicationStatus = application.Status.ToString(),
                                              JobTitle = job.Title,
                                              JobDescription = job.Description,
                                              JobLocation = job.Location,
                                              JobType = job.JobType.ToString(),
                                              SalaryRange = job.SalaryRange,
                                              CompanyName = company.Name,
                                              CompanyDescription = company.Description,
                                              CompanyWebsite = company.Website,
                                              PostedDate = DateOnly.FromDateTime(job.PostedDate)
                                          }).ToListAsync();

                return applications;
            }
            catch (Exception ex)
            {
               return new List<ApplicationDetails>();
            }
        }
        public async Task<bool> DeleteApplicationById(int applicationId, int? userId = null, bool isAdmin = false)
        {
            try
            {
                var query = context.Applications.AsQueryable();

                if (!isAdmin)
                {
                    query = query.Where(a => a.UserId == userId);
                }

                var application = await query
                    .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.Active);

                if (application == null)
                    return false;

                application.Active = false; // Soft delete
                context.Applications.Update(application);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string?> GetResumeUrlByApplicationId(int userId,int applicationId)
        {
            try
            {
                var resumeUrl = await context.Applications
               .Where(a => a.ApplicationId == applicationId && a.UserId == userId && a.Active)
               .Select(a => a.ResumeUrl) // Assuming ResumeUrl holds just the file name, not full path
               .FirstOrDefaultAsync();

                return resumeUrl;
            }
            catch (Exception ex)
            {
                return null;
            }
           
        }

        public async Task<bool> UpdateApplicationStatus(UpdateApplicationStatus updateApplicationStatus, int userId)
        {
            try
            {
                var query = await (
                    from application in context.Applications
                    join job in context.Jobs on application.JobId equals job.JobId
                    join company in context.Companies on job.CompanyId equals company.CompanyId
                    where application.ApplicationId == updateApplicationStatus.ApplicationId
                          && application.Active
                          && job.Active
                          && company.Active
                    select new
                    {
                        Application = application,
                        CompanyOwnerId = company.UserId
                    }
                ).FirstOrDefaultAsync();

                if (query == null)
                    return false;

                // Ensure only the company owner can update the application status
                if (query.CompanyOwnerId != userId)
                    return false;

                if (!System.Enum.TryParse<ApplicationStatus>(updateApplicationStatus.Status, true, out var statusEnum))
                    return false;

                query.Application.Status = statusEnum;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string?> GetUserEmailByApplicationId(int applicationId)
        {
            try
            {
                return await (
                       from app in context.Applications
                       join user in context.Users on app.UserId equals user.UserId
                       where app.ApplicationId == applicationId && user.Active
                       select user.Email
                       ).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                return "";
            }
           
        }
        public async Task<(string JobTitle, string CompanyName)?> GetJobAndCompanyNameByApplicationId(int applicationId)
        {
            try
            {
                var result = await (
                    from app in context.Applications
                    join job in context.Jobs on app.JobId equals job.JobId
                    join company in context.Companies on job.CompanyId equals company.CompanyId
                    where app.ApplicationId == applicationId && app.Active && job.Active && company.Active
                    select new { JobTitle = job.Title, CompanyName = company.Name }
                ).FirstOrDefaultAsync();

                if (result != null)
                    return (result.JobTitle, result.CompanyName);

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
