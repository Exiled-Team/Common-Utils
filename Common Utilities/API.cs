namespace Common_Utilities
{ 
    using System.Collections.Generic;

    using Exiled.API.Features;
    using PlayerRoles;

    /// <summary>
    /// Provides various utility methods for handling player roles, starting items, and health modifications in the game.
    /// </summary>
    public static class API
    {
        /// <summary>
        /// Gets the list of starting items for a specific role.
        /// </summary>
        /// <param name="role">The role type ID for which to get the starting items.</param>
        /// <returns>A list of <see cref="ItemType"/> representing the starting items for the specified role.</returns>
        public static List<ItemType> GetStartItems(RoleTypeId role) => Main.Instance.PlayerHandlers.StartItems(role);

        /// <summary>
        /// Gets the list of starting items for a specific role and player.
        /// </summary>
        /// <param name="role">The role type ID for which to get the starting items.</param>
        /// <param name="player">The player for whom to retrieve the starting items.</param>
        /// <returns>A list of <see cref="ItemType"/> representing the starting items for the specified player and role.</returns>
        public static List<ItemType> GetStartItems(RoleTypeId role, Player player) => Main.Instance.PlayerHandlers.StartItems(role, player);

        /// <summary>
        /// Gets the amount of health restored when a player with the specified role kills another player.
        /// </summary>
        /// <param name="role">The role for which to get the health on kill.</param>
        /// <returns>The amount of health restored on a kill, or 0 if none is configured for the specified role.</returns>
        public static float GetHealthOnKill(object role) => Main.Instance.Config.HealthOnKill?.ContainsKey(role) ?? false ? Main.Instance.Config.HealthOnKill[role] : 0f;
    }
}