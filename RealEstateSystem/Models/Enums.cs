using System;

namespace RealEstateSystem.Models
{
    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public enum UserRole
    {
        Admin = 1,
        Seller = 2,
        Buyer = 3
    }

    public enum SellerType
    {
        Agency = 1,
        Owner = 2
    }

    public enum RegistrationStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public enum PropertyType
    {
        Apartment = 1,
        House = 2,
        Commercial = 3,
        Land = 4
    }

    public enum PropertyStatus
    {
        Available = 1,
        UnderNegotiation = 2,
        Sold = 3,
        Rented = 4,
        Removed = 5
    }

    public enum PropertyApprovalStatus
    {
        Pending = 1,
        InReview = 2,
        Approved = 3,
        Rejected = 4
    }

    public enum DocumentType
    {
        Pdf = 1,
        Image = 2,
        Other = 3
    }

    public enum InquiryStatus
    {
        New = 1,
        Open = 2,
        Replied = 3,
        FollowUp = 4,
        Closed = 5
    }

    public enum AppointmentStatus
    {
        Requested = 1,
        Confirmed = 2,
        Rescheduled = 3,
        Cancelled = 4,
        Completed = 5
    }

    public enum OfferStatus
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3,
        CounterOffer = 4
    }

    public enum PaymentType
    {
        BookingFee = 1,
        Deposit = 2,
        FullPayment = 3
    }

    public enum PaymentMethod
    {
        CreditCard = 1,
        BankTransfer = 2,
        Cash = 3,
        MobileBanking = 4
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }

    public enum ActivityType
    {
        UserRegistration = 1,
        PropertyListing = 2,
        PropertyApproval = 3,
        PropertySold = 4,
        OfferMade = 5,
        OfferAccepted = 6,
        PaymentCompleted = 7,
        Login = 8,
        Other = 9
    }

    public enum ReportType
    {
        MonthlyListing = 1,
        PropertyViews = 2,
        SalesReport = 3,
        UserActivity = 4
    }

    public enum NotificationType
    {
        NewInquiry = 1,
        NewOffer = 2,
        AppointmentScheduled = 3,
        PropertyApproved = 4,
        PropertyRejected = 5,
        OfferAccepted = 6,
        OfferRejected = 7,
        PaymentReceived = 8,
        General = 9
    }
}
