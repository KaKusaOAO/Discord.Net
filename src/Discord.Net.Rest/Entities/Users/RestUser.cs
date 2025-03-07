using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using EventUserModel = Discord.API.GuildScheduledEventUser;
using Model = Discord.API.User;

namespace Discord.Rest
{
    /// <summary>
    ///     Represents a REST-based user.
    /// </summary>
    [DebuggerDisplay(@"{DebuggerDisplay,nq}")]
    public class RestUser : RestEntity<ulong>, IUser, IUpdateable
    {
        #region RestUser
        /// <inheritdoc />
        public bool IsBot { get; private set; }
        public bool IsSystem { get; private set; }
        /// <inheritdoc />
        public string Username { get; private set; }

        public virtual string DisplayName => GlobalName ?? Username;

        /// <inheritdoc />
        public ushort DiscriminatorValue => Discriminator != null
            ? ushort.Parse(Discriminator, CultureInfo.InvariantCulture)
            : throw new InvalidOperationException("Discriminator is unavailable");

        /// <inheritdoc />
        public string AvatarId { get; private set; }
        /// <inheritdoc />
        public string BannerId { get; private set; }
        /// <inheritdoc />
        public Color? AccentColor { get; private set; }
        /// <inheritdoc />
        public UserProperties? PublicFlags { get; private set; }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);
        /// <inheritdoc />
        public string Discriminator { get; private set; }

        /// <inheritdoc />
        public string Mention => MentionUtils.MentionUser(Id);
        /// <inheritdoc />
        public virtual IActivity Activity => null;
        /// <inheritdoc />
        public virtual UserStatus Status => UserStatus.Offline;
        /// <inheritdoc />
        public virtual IReadOnlyCollection<ClientType> ActiveClients => ImmutableHashSet<ClientType>.Empty;
        /// <inheritdoc />
        public virtual IReadOnlyCollection<IActivity> Activities => ImmutableList<IActivity>.Empty;
        /// <inheritdoc />
        public virtual bool IsWebhook => false;

        internal RestUser(BaseDiscordClient discord, ulong id)
            : base(discord, id)
        {
        }
        internal static RestUser Create(BaseDiscordClient discord, Model model)
            => Create(discord, null, model, null);
        internal static RestUser Create(BaseDiscordClient discord, IGuild guild, Model model, ulong? webhookId)
        {
            RestUser entity;
            if (webhookId.HasValue)
                entity = new RestWebhookUser(discord, guild, model.Id, webhookId.Value);
            else
                entity = new RestUser(discord, model.Id);
            entity.Update(model);
            return entity;
        }
        internal static RestUser Create(BaseDiscordClient discord, IGuild guild, EventUserModel model)
        {
            if (model.Member.IsSpecified)
            {
                var member = model.Member.Value;
                member.User = model.User;
                return RestGuildUser.Create(discord, guild, member);
            }
            else
                return RestUser.Create(discord, model.User);
        }

        internal virtual void Update(Model model)
        {
            if (model.Avatar.IsSpecified)
                AvatarId = model.Avatar.Value;
            if (model.Banner.IsSpecified)
                BannerId = model.Banner.Value;
            if (model.AccentColor.IsSpecified)
                AccentColor = model.AccentColor.Value;
            if (model.Discriminator.IsSpecified)
                Discriminator = model.Discriminator.Value;
            if (model.Bot.IsSpecified)
                IsBot = model.Bot.Value;
            if (model.System.IsSpecified)
                IsSystem = model.System.Value;
            if (model.Username.IsSpecified)
                Username = model.Username.Value;
            if (model.PublicFlags.IsSpecified)
                PublicFlags = model.PublicFlags.Value;
            if (model.GlobalName.IsSpecified)
                GlobalName = model.GlobalName.Value;
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(RequestOptions options = null)
        {
            var model = await Discord.ApiClient.GetUserAsync(Id, options).ConfigureAwait(false);
            Update(model);
        }

        /// <summary>
        ///     Creates a direct message channel to this user.
        /// </summary>
        /// <param name="options">The options to be used when sending the request.</param>
        /// <returns>
        ///     A task that represents the asynchronous get operation. The task result contains a rest DM channel where the user is the recipient.
        /// </returns>
        public Task<RestDMChannel> CreateDMChannelAsync(RequestOptions options = null)
            => UserHelper.CreateDMChannelAsync(this, Discord, options);
        public string GlobalName { get; internal set; }

        /// <inheritdoc />
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => CDN.GetUserAvatarUrl(Id, AvatarId, size, format);

        /// <inheritdoc />
        public string GetBannerUrl(ImageFormat format = ImageFormat.Auto, ushort size = 480)
            => CDN.GetUserBannerUrl(Id, BannerId, size, format);

        /// <inheritdoc />
        public string GetDefaultAvatarUrl()
            => this.HasLegacyUsername() ?
                CDN.GetDefaultUserAvatarUrl(DiscriminatorValue) :
                CDN.GetDefaultUserAvatarUrl(Id);

        /// <summary>
        ///     Gets the Username#Discriminator of the user.
        /// </summary>
        /// <returns>
        ///     A string that resolves to Username#Discriminator of the user.
        /// </returns>
        public override string ToString()
            => Format.UsernameAndDiscriminator(this, Discord.FormatUsersInBidirectionalUnicode);

        private string DebuggerDisplay => $"{Format.UsernameAndDiscriminator(this, Discord.FormatUsersInBidirectionalUnicode)} ({Id}{(IsBot ? ", Bot" : "")})";
        #endregion

        #region IUser
        /// <inheritdoc />
        async Task<IDMChannel> IUser.CreateDMChannelAsync(RequestOptions options)
            => await CreateDMChannelAsync(options).ConfigureAwait(false);
        #endregion
    }
}
