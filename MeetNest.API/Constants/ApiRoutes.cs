namespace MeetNest.API.Constants;

public static class ApiRoutes
{
    public const string Base = "/api";

    public static class Auth
    {
        public const string Login = Base + "/auth/login";
        public const string RegisterAdmin = Base + "/auth/register/admin";
        public const string RegisterEmployee = Base + "/auth/register/employee";
        public const string RefreshToken = Base + "/auth/refresh-token";
    }

    public static class Branch
    {
        public const string Create = Base + "/branches";
        public const string GetAll = Base + "/branches";
        public const string GetById = Base + "/branches/{id}";
        public const string Update = Base + "/branches/{id}";
        public const string Delete = Base + "/branches/{id}";

        // Admin: branches with employee + room counts
        public const string GetAllWithStats = Base + "/branches/stats";
    }

    public static class Room
    {
        public const string Create = Base + "/rooms";
        public const string GetAll = Base + "/rooms";
        public const string GetByBranch = Base + "/rooms/branch/{branchId}";
        public const string GetById = Base + "/rooms/{id}";
        public const string Update = Base + "/rooms/{id}";
        public const string Delete = Base + "/rooms/{id}";
        public const string EmployeeRooms = Base + "/rooms/employee";
    }

    public static class Facility
    {
        public const string Create = Base + "/facilities";
        public const string GetAll = Base + "/facilities";
        public const string GetById = Base + "/facilities/{id}";
        public const string Update = Base + "/facilities/{id}";
        public const string Delete = Base + "/facilities/{id}";
    }

    public static class RoomFacility
    {
        public const string Assign = Base + "/room-facilities/assign";
        public const string GetByRoom = Base + "/room-facilities/room/{roomId}";
        public const string Remove = Base + "/room-facilities/remove";
    }

    public static class Booking
    {
        // Employee
        public const string Create = Base + "/bookings";
        public const string MyBookings = Base + "/bookings/my";
        public const string Cancel = Base + "/bookings/{id}/cancel";

        // Admin (existing — kept for backward compat)
        public const string Approve = Base + "/bookings/{id}/approve";
        public const string Reject = Base + "/bookings/{id}/reject";
    }

    // ── New Admin Routes ──────────────────────────────────────────────────────

    public static class Admin
    {
        // Dashboard stats
        public const string Dashboard = Base + "/admin/dashboard";

        // Admin Booking Management
        public const string BookingsGetAll = Base + "/admin/bookings";
        public const string BookingGetById = Base + "/admin/bookings/{id}";
        public const string BookingApprove = Base + "/admin/bookings/{id}/approve";
        public const string BookingReject = Base + "/admin/bookings/{id}/reject";

        // User Management
        public const string UsersGetAll = Base + "/admin/users";
        public const string UsersGetEmployees = Base + "/admin/users/employees";
        public const string UserGetById = Base + "/admin/users/{id}";
        public const string UserUpdate = Base + "/admin/users/{id}";
        public const string UserDeactivate = Base + "/admin/users/{id}/deactivate";
        public const string UserResetPassword = Base + "/admin/users/{id}/reset-password";
    }
}
