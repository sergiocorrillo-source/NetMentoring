namespace Ticketing.Tests
{
    /// <summary>
    /// Configuración global para las pruebas unitarias
    /// </summary>
    public static class TestConstants
    {
        // GUIDs de prueba
        public static readonly Guid TEST_CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid TEST_EVENT_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid TEST_ORDER_ID = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public static readonly Guid TEST_SEAT_ID = Guid.Parse("44444444-4444-4444-4444-444444444444");
        public static readonly Guid TEST_TICKET_ID = Guid.Parse("55555555-5555-5555-5555-555555555555");

        // Datos de prueba
        public const string TEST_EMAIL = "test@example.com";
        public const string TEST_CUSTOMER_NAME = "Test Customer";
        public const string TEST_EVENT_NAME = "Test Event";
        public const decimal TEST_AMOUNT = 100.00m;
        public const string TEST_CURRENCY = "USD";

        // Estados válidos
        public const string ORDER_STATUS_CREATED = "Created";
        public const string ORDER_STATUS_PENDING = "PendingPayment";
        public const string ORDER_STATUS_PAID = "Paid";
        public const string ORDER_STATUS_CANCELLED = "Cancelled";

        // Códigos HTTP
        public const int HTTP_OK = 200;
        public const int HTTP_CREATED = 201;
        public const int HTTP_NO_CONTENT = 204;
        public const int HTTP_BAD_REQUEST = 400;
        public const int HTTP_NOT_FOUND = 404;
        public const int HTTP_INTERNAL_SERVER_ERROR = 500;
    }
}
