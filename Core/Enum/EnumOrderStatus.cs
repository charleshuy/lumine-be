using System.Text.Json.Serialization;

namespace Core.Constants
{
    /// <summary>
    /// Defines the different statuses of an order.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumOrderStatus
    {
        /// <summary>
        /// The order is in the cart and not yet confirmed.
        /// </summary>
        Cart = 0,

        /// <summary>
        /// The order has been confirmed by the system.
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// The order is being prepared.
        /// </summary>
        Preparing = 2,

        // /// <summary>
        // /// The order is being delivered.
        // /// </summary>
        // Delivering = 3,

        /// <summary>
        /// The order is completed successfully.
        /// </summary>
        Done = 4,

        /// <summary>
        /// The order failed or was canceled.
        /// </summary>
        Canceled = 5
    }
}
