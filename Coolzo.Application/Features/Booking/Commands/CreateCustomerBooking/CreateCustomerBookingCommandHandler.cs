using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;
using DomainBooking = Coolzo.Domain.Entities.Booking;

namespace Coolzo.Application.Features.Booking.Commands.CreateCustomerBooking;

public sealed class CreateCustomerBookingCommandHandler : IRequestHandler<CreateCustomerBookingCommand, BookingSummaryResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly IBookingReferenceGenerator _bookingReferenceGenerator;
    private readonly IBookingRepository _bookingRepository;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateCustomerBookingCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerBookingCommandHandler(
        IBookingLookupRepository bookingLookupRepository,
        IBookingRepository bookingRepository,
        IBookingReferenceGenerator bookingReferenceGenerator,
        IAuditLogRepository auditLogRepository,
        ISystemSettingRepository systemSettingRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<CreateCustomerBookingCommandHandler> logger)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _bookingRepository = bookingRepository;
        _bookingReferenceGenerator = bookingReferenceGenerator;
        _auditLogRepository = auditLogRepository;
        _systemSettingRepository = systemSettingRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<BookingSummaryResponse> Handle(CreateCustomerBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? null : request.IdempotencyKey.Trim();

        if (idempotencyKey is not null)
        {
            var existingBooking = await _bookingRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);

            if (existingBooking is not null)
            {
                _logger.LogInformation("Customer booking idempotency key {IdempotencyKey} reused for {BookingReference}.", idempotencyKey, existingBooking.BookingReference);
                return BookingResponseMapper.ToSummary(existingBooking);
            }
        }

        var service = await _bookingLookupRepository.GetServiceByIdAsync(request.ServiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected service is invalid.", 400);

        var acType = await _bookingLookupRepository.GetAcTypeByIdAsync(request.AcTypeId, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected AC type is invalid.", 400);

        // Tonnage/Brand are optional — verified on-site by the technician. Validate only when supplied.
        Tonnage? tonnage = null;
        if (request.TonnageId.HasValue)
        {
            tonnage = await _bookingLookupRepository.GetTonnageByIdAsync(request.TonnageId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected tonnage is invalid.", 400);
        }

        Brand? brand = null;
        if (request.BrandId.HasValue)
        {
            brand = await _bookingLookupRepository.GetBrandByIdAsync(request.BrandId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The selected brand is invalid.", 400);
        }

        var zone = await _bookingLookupRepository.GetZoneByPincodeAsync(request.Pincode, cancellationToken)
            ?? throw new AppException(ErrorCodes.ZoneNotServed, "The provided pincode is not serviceable.", 404);

        var bookingFlags = await _systemSettingRepository.GetByKeysAsync(
            new[] { "Booking.OpenBookingMode", "Booking.EnforceSlotCapacity" },
            cancellationToken);

        var openBookingMode = bookingFlags.TryGetValue("Booking.OpenBookingMode", out var obm)
            && bool.TryParse(obm.SettingValue, out var obmVal) && obmVal;

        var enforceSlotCapacity = !bookingFlags.TryGetValue("Booking.EnforceSlotCapacity", out var esc)
            || !bool.TryParse(esc.SettingValue, out var escVal) || escVal;

        // When OpenBookingMode is on, slot selection is optional — admin assigns time later.
        // When EnforceSlotCapacity is off, capacity limits are bypassed.
        SlotAvailability? slotAvailability = null;
        if (request.SlotAvailabilityId.HasValue)
        {
            slotAvailability = await _bookingLookupRepository.GetSlotAvailabilityByIdAsync(request.SlotAvailabilityId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot is unavailable.", 409);

            if (enforceSlotCapacity)
            {
                ValidateSlot(zone.ZoneId, slotAvailability);
            }
        }
        else if (!request.IsEmergency && !openBookingMode)
        {
            throw new AppException(ErrorCodes.SlotUnavailable, "A time slot is required.", 400);
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (customer is null)
        {
            customer = new Customer
            {
                UserId = _currentUserContext.UserId,
                CustomerName = request.CustomerName?.Trim() ?? _currentUserContext.UserName,
                MobileNumber = request.MobileNumber?.Trim() ?? string.Empty,
                EmailAddress = request.EmailAddress?.Trim() ?? string.Empty,
                IsGuestCustomer = false,
                IsActive = true,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            };

            await _bookingRepository.AddCustomerAsync(customer, cancellationToken);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.CustomerName)) customer.CustomerName = request.CustomerName.Trim();
            if (!string.IsNullOrWhiteSpace(request.MobileNumber)) customer.MobileNumber = request.MobileNumber.Trim();
            if (!string.IsNullOrWhiteSpace(request.EmailAddress)) customer.EmailAddress = request.EmailAddress.Trim();
            customer.IsGuestCustomer = false;
            customer.LastUpdated = _currentDateTime.UtcNow;
            customer.UpdatedBy = _currentUserContext.UserName;
        }

        var customerAddress = await _bookingRepository.GetCustomerAddressAsync(
            customer.CustomerId,
            request.AddressLine1.Trim(),
            request.Pincode.Trim(),
            cancellationToken);

        if (customerAddress is null)
        {
            customerAddress = new CustomerAddress
            {
                Customer = customer,
                ZoneId = zone.ZoneId,
                AddressLabel = request.AddressLabel?.Trim() ?? "Saved Address",
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 = request.AddressLine2?.Trim() ?? string.Empty,
                Landmark = request.Landmark?.Trim() ?? string.Empty,
                CityName = request.CityName.Trim(),
                Pincode = request.Pincode.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsDefault = customer.CustomerAddresses.Count == 0,
                IsActive = true,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            };

            await _bookingRepository.AddCustomerAddressAsync(customerAddress, cancellationToken);
        }
        else
        {
            customerAddress.ZoneId = zone.ZoneId;
            customerAddress.AddressLine2 = request.AddressLine2?.Trim() ?? string.Empty;
            customerAddress.Landmark = request.Landmark?.Trim() ?? string.Empty;
            customerAddress.CityName = request.CityName.Trim();
            customerAddress.AddressLabel = request.AddressLabel?.Trim() ?? customerAddress.AddressLabel;
            if (request.Latitude.HasValue) customerAddress.Latitude = request.Latitude;
            if (request.Longitude.HasValue) customerAddress.Longitude = request.Longitude;
            customerAddress.LastUpdated = _currentDateTime.UtcNow;
            customerAddress.UpdatedBy = _currentUserContext.UserName;
        }

        var bookingReference = await GenerateUniqueBookingReferenceAsync(cancellationToken);
        var sourceChannel = Enum.Parse<BookingSourceChannel>(request.SourceChannel, true);

        if (slotAvailability is not null)
        {
            slotAvailability.ReservedCapacity += 1;
        }

        var booking = new DomainBooking
        {
            BookingReference = bookingReference,
            IdempotencyKey = idempotencyKey,
            Customer = customer,
            CustomerAddress = customerAddress,
            ZoneId = zone.ZoneId,
            SlotAvailability = slotAvailability,
            BookingDateUtc = _currentDateTime.UtcNow,
            BookingStatus = BookingStatus.Confirmed,
            SourceChannel = sourceChannel,
            IsGuestBooking = false,
            IsEmergency = request.IsEmergency,
            EmergencySurchargeAmount = request.EmergencySurchargeAmount ?? 0,
            CustomerNameSnapshot = customer.CustomerName,
            MobileNumberSnapshot = customer.MobileNumber,
            EmailAddressSnapshot = customer.EmailAddress,
            AddressLine1Snapshot = customerAddress.AddressLine1,
            AddressLine2Snapshot = customerAddress.AddressLine2,
            LandmarkSnapshot = customerAddress.Landmark,
            CityNameSnapshot = customerAddress.CityName,
            PincodeSnapshot = customerAddress.Pincode,
            ZoneNameSnapshot = zone.ZoneName,
            ServiceNameSnapshot = service.ServiceName,
            LatitudeSnapshot = request.Latitude,
            LongitudeSnapshot = request.Longitude,
            EstimatedPrice = service.BasePrice + (request.EmergencySurchargeAmount ?? 0),
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        booking.BookingLines.Add(new BookingLine
        {
            ServiceId = service.ServiceId,
            AcTypeId = acType.AcTypeId,
            TonnageId = tonnage?.TonnageId,
            BrandId = brand?.BrandId,
            ModelName = request.ModelName?.Trim() ?? string.Empty,
            IssueNotes = request.IssueNotes?.Trim() ?? string.Empty,
            Quantity = 1,
            UnitPrice = service.BasePrice + (request.EmergencySurchargeAmount ?? 0),
            LineTotal = service.BasePrice + (request.EmergencySurchargeAmount ?? 0),
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        });

        booking.BookingStatusHistories.Add(new BookingStatusHistory
        {
            BookingStatus = BookingStatus.Confirmed,
            Remarks = "Customer booking confirmed.",
            StatusDateUtc = _currentDateTime.UtcNow,
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        });

        var hasEquipment = await _bookingRepository.HasCustomerEquipmentByTypeAsync(
            customer.CustomerId, acType.AcTypeName, cancellationToken);

        if (!hasEquipment)
        {
            await _bookingRepository.AddCustomerEquipmentAsync(new CustomerEquipment
            {
                Customer = customer,
                EquipmentName = $"{acType.AcTypeName} AC",
                EquipmentType = acType.AcTypeName,
                BrandName = string.Empty,
                IsActive = true,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            }, cancellationToken);
        }

        await _bookingRepository.AddBookingAsync(booking, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateCustomerBooking",
                EntityName = "Booking",
                EntityId = bookingReference,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = bookingReference,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer booking {BookingReference} created by user {UserId}.", bookingReference, _currentUserContext.UserId);

        return BookingResponseMapper.ToSummary(booking);
    }

    private static void ValidateSlot(long zoneId, SlotAvailability slotAvailability)
    {
        if (slotAvailability.ZoneId != zoneId || slotAvailability.IsBlocked || slotAvailability.ReservedCapacity >= slotAvailability.AvailableCapacity)
        {
            throw new AppException(ErrorCodes.SlotUnavailable, "The selected slot is unavailable.", 409);
        }
    }

    private async Task<string> GenerateUniqueBookingReferenceAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var bookingReference = _bookingReferenceGenerator.GenerateReference();

            if (!await _bookingRepository.BookingReferenceExistsAsync(bookingReference, cancellationToken))
            {
                return bookingReference;
            }
        }
    }
}
