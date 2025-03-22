// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace MRTK_Custom.Dock
{
    /// <summary>
    /// The possible states of a <see cref="Microsoft.MixedReality.Toolkit.Experimental.UI.Dockable"/> object.
    /// </summary>
    public enum DockingState
    {
        Undocked = 0,
        Docking,
        Docked,
        Undocking
    }
}