using Application.DTOs.ServiceDTO;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using AutoMapper;
using Domain.Base;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ServiceTypeService : IServiceTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceTypeDTO>> GetAllAsync()
        {
            var types = await _unitOfWork.GetRepository<ServiceType>()
                .Entities
                .Where(st => !st.IsDeleted)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceTypeDTO>>(types);
        }

        public async Task<ServiceTypeDTO> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.GetRepository<ServiceType>().GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
            {
                throw new BaseException.NotFoundException("service_type_not_found", $"Service type with ID {id} not found.");
            }

            return _mapper.Map<ServiceTypeDTO>(entity);
        }

        public async Task<ServiceTypeDTO> CreateAsync(CreateServiceTypeDTO dto)
        {
            var entity = _mapper.Map<ServiceType>(dto);
            await _unitOfWork.GetRepository<ServiceType>().InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ServiceTypeDTO>(entity);
        }

        public async Task UpdateAsync(UpdateServiceTypeDTO dto)
        {
            var repo = _unitOfWork.GetRepository<ServiceType>();
            var entity = await repo.GetByIdAsync(dto.Id);

            if (entity == null || entity.IsDeleted)
            {
                throw new BaseException.NotFoundException("service_type_not_found", $"Service type with ID {dto.Id} not found.");
            }

            entity.Name = dto.Name;
            entity.Description = dto.Description;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<ServiceType>();
            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
            {
                throw new BaseException.NotFoundException("service_type_not_found", $"Service type with ID {id} not found.");
            }

            entity.IsDeleted = true;
            repo.Update(entity);
            await _unitOfWork.SaveAsync();
        }
    }
}
