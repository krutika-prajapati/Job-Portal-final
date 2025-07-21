using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Model;
using Shared.DTO;

namespace Repository.Repository
{
    public class CompanyRepo : ICompanyRepo
    {
        private readonly DBContext context;

        public CompanyRepo(DBContext context)
        {
            this.context = context;
        }

        public async Task<bool> CreateCompany(Company company)
        {
            try
            {
                using (context)
                {
                    await context.Companies.AddAsync(company);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;

            }
        }

        public async Task<bool> CompanyAlreadyExists(string companyName)
        {
            try
            {
                using (context)
                {
                    if (await context.Companies.AnyAsync(company => company.Name==companyName && company.Active))
                    {
                        return true; // Company with this name already exists
                    }
                    else
                    {
                        return false; // Company with this name does not exist
                    }
                }
            }
            catch (Exception)
            {
                return false; // An error occurred while checking for the company
            }
        }

        public async Task<bool> DeleteCompany(int userId, int companyId, bool isAdmin)
        {
            try
            {
                using (context)
                {
                    var company = await context.Companies.FindAsync(companyId);

                    if (company == null)
                    {
                        return false; // Company not found
                    }

                    if (company.UserId != userId && !isAdmin)
                    {
                        return false; // User does not have permission to delete this company
                    }

                    company.Active = false; // Soft delete
                    context.Companies.Update(company);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false; // An error occurred while deleting the company
            }
        }

        public async Task<List<CompanyWithUser>> GetAllCompanyWithUserDetails()
        {
            try
            {
                return await (
                    from user in context.Users
                    join company in context.Companies
                    on user.UserId equals company.UserId
                    where user.Active && company.Active
                    select new CompanyWithUser
                    {
                        Id = company.CompanyId,
                        CompanyName = company.Name,
                        Description = company.Description,
                        Website = company.Website,
                        EmployerEmail = user.Email,
                        EmployerName = user.FullName,
                        EmployerPhone = user.Phone
                    }
                ).ToListAsync();
            }
            catch (Exception ex)
            {

                return new List<CompanyWithUser>();
            }
        }

        public async Task<List<CompanyWithUser>> GetCompanyCreatedByUser(int userId)
        {
            try
            {
                return await (
                   from user in context.Users
                   join company in context.Companies
                   on user.UserId equals company.UserId
                   where user.Active && company.Active && user.UserId==userId
                   select new CompanyWithUser
                   {
                       Id = company.CompanyId,
                       CompanyName = company.Name,
                       Description = company.Description,
                       Website = company.Website,
                       EmployerEmail = user.Email,
                       EmployerName = user.FullName,
                       EmployerPhone = user.Phone
                   }
               ).ToListAsync();
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public async Task<bool> UpdateCompany(int userId, UpdateCompany updatedCompany, bool isAdmin)
        {
            try
            {
                var company = await context.Companies.FindAsync(updatedCompany.CompanyId);
                if (company == null)
                {
                    return false; // Company not found
                }

                if (company.UserId != userId && !isAdmin)
                {
                    return false; // User does not have permission to update this company
                }

                // Update the properties
                company.Name = updatedCompany.CompanyName;
                company.Description = updatedCompany.Description;
                company.Website = updatedCompany.Website;

                context.Companies.Update(company);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false; // An error occurred while updating the company
            }
        }

    }
}
