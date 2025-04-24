using Application.DTOs.ServiceDTO;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using Application.Paggings;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Domain.Base.BaseException;

namespace Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedList<ResponseServiceDTO>> GetAllServicesAsync(
            int pageIndex,
            int pageSize,
            string? serviceName = null,
            decimal? price = null,
            ServiceStatus? status = null)
        {
            var query = _unitOfWork.GetRepository<Service>()
                .Entities
                .Include(s => s.Artist)
                .Include(s => s.ServiceType)
                .Where(s => !s.IsDeleted);

            // Apply filters if provided
            if (!string.IsNullOrEmpty(serviceName))
            {
                query = query.Where(s => s.ServiceName.Contains(serviceName));
            }

            if (price.HasValue)
            {
                query = query.Where(s => s.Price >= price.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            // Order the results
            query = query.OrderBy(s => s.ServiceName);

            // Get paginated results
            var paged = await _unitOfWork.GetRepository<Service>().GetPagging(query, pageIndex, pageSize);
            var dtoList = _mapper.Map<List<ResponseServiceDTO>>(paged.Items);

            return new PaginatedList<ResponseServiceDTO>(dtoList, paged.TotalCount, pageIndex, pageSize);
        }

        public async Task<PaginatedList<ResponseServiceDTO>> GetAllServicesByArtistIdAsync(int pageIndex, int pageSize, Guid artistId)
        {
            var query = _unitOfWork.GetRepository<Service>()
                .Entities
                .Include(s => s.Artist)
                .Include(s => s.ServiceType)
                .Where(s => !s.IsDeleted && s.ArtistID == artistId)
                .OrderBy(s => s.ServiceName);

            var paged = await _unitOfWork.GetRepository<Service>().GetPagging(query, pageIndex, pageSize);
            var dtoList = _mapper.Map<List<ResponseServiceDTO>>(paged.Items);

            return new PaginatedList<ResponseServiceDTO>(dtoList, paged.TotalCount, pageIndex, pageSize);
        }

        public async Task<ResponseServiceDTO> CreateAsync(CreateServiceDTO dto)
        {
            var entity = _mapper.Map<Service>(dto);

            await _unitOfWork.GetRepository<Service>().InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ResponseServiceDTO>(entity);
        }

        public async Task UpdateAsync(UpdateServiceDTO dto)
        {
            var repo = _unitOfWork.GetRepository<Service>();
            var entity = await repo.GetByIdAsync(dto.Id);

            if (entity == null || entity.IsDeleted)
                throw new NotFoundException("service_not_found", $"Service with ID {dto.Id} not found");

            _mapper.Map(dto, entity);

            await repo.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<Service>();
            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                throw new NotFoundException("service_not_found", $"Service with ID {id} not found");

            entity.IsDeleted = true;
            await repo.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
        }
    }
}
