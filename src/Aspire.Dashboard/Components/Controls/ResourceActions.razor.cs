// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Aspire.Dashboard.Components;

public partial class ResourceActions : ComponentBase
{
    private static readonly Icon s_viewDetailsIcon = new Icons.Regular.Size16.Info();
    private static readonly Icon s_consoleLogsIcon = new Icons.Regular.Size16.SlideText();

    private AspireMenuButton? _menuButton;

    [Inject]
    public required IStringLocalizer<Resources.Resources> Loc { get; set; }

    [Inject]
    public required IStringLocalizer<Resources.ControlsStrings> ControlLoc { get; set; }

    [Parameter]
    public required IList<CommandViewModel> Commands { get; set; }

    [Parameter]
    public required EventCallback<CommandViewModel> CommandSelected { get; set; }

    [Parameter]
    public required EventCallback<string> OnViewDetails { get; set; }

    [Parameter]
    public required EventCallback OnConsoleLogs { get; set; }

    [Parameter]
    public required int MaxHighlightedCount { get; set; }

    [CascadingParameter]
    public required ViewportInformation ViewportInformation { get; set; }

    private readonly List<CommandViewModel> _highlightedCommands = new();
    private readonly List<MenuButtonItem> _menuItems = new();

    protected override void OnParametersSet()
    {
        _menuItems.Clear();
        _highlightedCommands.Clear();

        _menuItems.Add(new MenuButtonItem
        {
            Text = ControlLoc[nameof(Resources.ControlsStrings.ActionViewDetailsText)],
            Icon = s_viewDetailsIcon,
            OnClick = () => OnViewDetails.InvokeAsync(_menuButton?.MenuButtonId)
        });
        _menuItems.Add(new MenuButtonItem
        {
            Text = Loc[nameof(Resources.Resources.ResourceActionConsoleLogsText)],
            Icon = s_consoleLogsIcon,
            OnClick = OnConsoleLogs.InvokeAsync
        });

        if (ViewportInformation.IsDesktop)
        {
            _highlightedCommands.AddRange(Commands.Where(c => c.IsHighlighted && c.State != CommandViewModelState.Hidden).Take(MaxHighlightedCount));
        }

        var menuCommands = Commands.Where(c => !_highlightedCommands.Contains(c) && c.State != CommandViewModelState.Hidden).ToList();
        if (menuCommands.Count > 0)
        {
            _menuItems.Add(new MenuButtonItem { IsDivider = true });

            foreach (var command in menuCommands)
            {
                var icon = (!string.IsNullOrEmpty(command.IconName) && CommandViewModel.ResolveIconName(command.IconName, command.IconVariant) is { } i) ? i : null;

                _menuItems.Add(new MenuButtonItem
                {
                    Text = command.DisplayName,
                    Tooltip = command.DisplayDescription,
                    Icon = icon,
                    OnClick = () => CommandSelected.InvokeAsync(command),
                    IsDisabled = command.State == CommandViewModelState.Disabled
                });
            }
        }
    }
}
