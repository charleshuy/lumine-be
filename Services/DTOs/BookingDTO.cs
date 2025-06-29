﻿using Application.DTOs.ServiceDTO;
using Domain.Entities;

namespace Application.DTOs
{
    public class BookingDTO
    {
        public Guid Id { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Deposit { get; set; }
        public string? Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BookingServiceDTO? Service { get; set; } 
        public CustomerDTO? Customer { get; set; }
    }

    public class CustomerDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class BookingCreateDTO
    {
        public Guid ServiceID { get; set; }
        public Guid CustomerID { get; set; }
        public DateTime StartTime { get; set; }
        //public DateTime EndTime { get; set; }
    }

    public class BookingStatusSummaryDTO
    {
        public DateTime Date { get; set; }
        public int CompletedCount { get; set; }
        public int CanceledCount { get; set; }
    }
}
