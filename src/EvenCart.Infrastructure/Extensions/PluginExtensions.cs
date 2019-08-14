﻿using System;
using System.Collections.Generic;
using System.Linq;
using EvenCart.Core.Infrastructure;
using EvenCart.Data.Entity.Settings;
using EvenCart.Data.Extensions;
using EvenCart.Services.Serializers;
using EvenCart.Services.Settings;
using EvenCart.Infrastructure.Plugins;
using EvenCart.Services.Payments;
using EvenCart.Services.Plugins;

namespace EvenCart.Infrastructure.Extensions
{
    public static class PluginExtensions
    {
        public static IList<PluginStatus> GetSitePlugins(this PluginSettings pluginSettings)
        {
            var sitePlugins = pluginSettings.SitePlugins;
            if (sitePlugins.IsNullEmptyOrWhiteSpace())
                return new List<PluginStatus>();

            var dataSerializer = DependencyResolver.Resolve<IDataSerializer>();
            return dataSerializer.DeserializeAs<IList<PluginStatus>>(sitePlugins);
        }

        public static void SetSitePlugins(this PluginSettings pluginSettings, IList<PluginStatus> pluginStatuses, bool save = false)
        {
            var dataSerializer = DependencyResolver.Resolve<IDataSerializer>();
            var sitePlugins = dataSerializer.Serialize(pluginStatuses);
            pluginSettings.SitePlugins = sitePlugins;
            if (save)
            {
                var settingService = DependencyResolver.Resolve<ISettingService>();
                settingService.Save(pluginSettings);
            }
        }

        public static void UpdatePluginStatus(this PluginSettings pluginSettings, string systemName, bool installed,
            bool active)
        {
            var pluginStatus = pluginSettings.GetSitePlugins();
            var ps = pluginStatus.FirstOrDefault(x => x.PluginSystemName == systemName);
            if (ps == null)
            {
                ps = new PluginStatus()
                {
                    PluginSystemName = systemName
                };
                pluginStatus.Add(ps);
            }
            ps.Active = active;
            ps.Installed = installed;
            pluginSettings.SetSitePlugins(pluginStatus, true);
        }

        public static IList<WidgetStatus> GetSiteWidgets(this PluginSettings pluginSettings)
        {
            var siteWidgets = pluginSettings.SiteWidgets;
            if (siteWidgets.IsNullEmptyOrWhiteSpace())
                return new List<WidgetStatus>();

            var dataSerializer = DependencyResolver.Resolve<IDataSerializer>();
            return dataSerializer.DeserializeAs<IList<WidgetStatus>>(siteWidgets).OrderBy(x => x.DisplayOrder).ToList();
        }

        public static void AddWidget(this PluginSettings pluginSettings, string widgetName, string pluginSystemName, string zoneName)
        {
            var siteWidgets = pluginSettings.GetSiteWidgets();
            //find widget count in zone
            var zoneWidgetCount = siteWidgets.Count(x => x.ZoneName == zoneName);
            siteWidgets.Add(new WidgetStatus()
            {
                WidgetSystemName = widgetName,
                PluginSystemName = pluginSystemName,
                ZoneName = zoneName,
                DisplayOrder = zoneWidgetCount,
                Id = Guid.NewGuid().ToString()
            });

            pluginSettings.SaveWidgets(siteWidgets, true);
        }

        public static void SaveWidgets(this PluginSettings pluginSettings, IList<WidgetStatus> widgets, bool save = false)
        {
            var dataSerializer = DependencyResolver.Resolve<IDataSerializer>();
            var sitePlugins = dataSerializer.Serialize(widgets);
            pluginSettings.SiteWidgets = sitePlugins;
            if (save)
            {
                var settingService = DependencyResolver.Resolve<ISettingService>();
                settingService.Save(pluginSettings);
            }
        }

        public static bool Supports(this IPaymentHandlerPlugin plugin, PaymentOperation operation)
        {
            return plugin.SupportedOperations.Contains(operation);
        }
    }
}