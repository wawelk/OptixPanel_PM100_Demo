#region Using directives
using FTOptix.Core;
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.OPCUAServer;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
#endregion

public class IdleTimeoutLogic : BaseNetLogic
{
    public override void Start()
    {
        duration = LogicObject.GetVariable("Duration");
        if (duration == null)
            throw new CoreConfigurationException("Unable to find Duration variable");

        enabled = LogicObject.GetVariable("Enabled");
        if (enabled == null)
            throw new CoreConfigurationException("Unable to find Enabled variable");

        onTimeout = (MethodInvocation)LogicObject.Get("OnTimeout");
        if (onTimeout == null)
            throw new CoreConfigurationException("Unable to find OnTimeout method invocation");

        uiSession = Session as UISession;
        if (uiSession == null)
            throw new CoreConfigurationException("Idle Timeout logic must be placed inside a UI object");

        enabled.VariableChange += Enabled_VariableChange;
        duration.VariableChange += Duration_VariableChange;

        uiSession.OnIdleTimeout += UiSession_OnIdleTimeout;
        uiSession.IdleTimeoutEnabled = enabled.Value;
        uiSession.IdleTimeoutDuration = TimeSpan.FromMilliseconds(duration.Value);
    }

    /// <summary>
    /// This method enables or disables idle timeout based on the new value received from VariableChangeEventArgs.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event data containing the new value.</param>
    /// <remarks>
    /// Event handler for variable changes related to idle timeout.
    /// </remarks>
    private void Enabled_VariableChange(object sender, VariableChangeEventArgs e)
    {
        uiSession.IdleTimeoutEnabled = e.NewValue;
    }

    /// <summary>
    /// This method updates the idle timeout duration based on the new value received during a variable change event.
    /// <example>
    /// For instance:
    /// <code>
    /// Duration_VariableChange(sender, e);
    /// </code>
    /// will set the idle timeout duration to <c>e.NewValue</c>.
    /// </example>
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments containing the new value for the idle timeout duration.</param>
    private void Duration_VariableChange(object sender, VariableChangeEventArgs e)
    {
        uiSession.IdleTimeoutDuration = TimeSpan.FromMilliseconds(e.NewValue);
    }

    /// <summary>
    /// The method is invoked when an idle timeout occurs during the UI session.
    /// <example>
    /// For example:
    /// <code>
    /// UiSession.OnIdleTimeout(null, new IdleTimeoutEventArgs());
    /// </code>
    /// triggers the <c>onTimeout</c> event handler.
    /// </example>
    /// </summary>
    /// <param name="sender">The source object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void UiSession_OnIdleTimeout(object sender, IdleTimeoutEvent e)
    {
        onTimeout.Invoke();
    }

    public override void Stop()
    {
        if (uiSession != null)
            uiSession.OnIdleTimeout -= UiSession_OnIdleTimeout;
        if (enabled != null)
            enabled.VariableChange -= Enabled_VariableChange;
        if (duration != null)
            duration.VariableChange -= Duration_VariableChange;
    }

    private UISession uiSession;
    private IUAVariable duration;
    private IUAVariable enabled;
    private MethodInvocation onTimeout;
}
