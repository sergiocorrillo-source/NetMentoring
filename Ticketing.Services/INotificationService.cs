using System.Threading.Tasks;
using Ticketing.Services.DTOs;
using System.Collections.Generic;
using System;

namespace Ticketing.Services
{
    public interface INotificationService
    {
        Task<Guid> EnqueueNotificationAsync(NotificationDto dto);
    }
}
