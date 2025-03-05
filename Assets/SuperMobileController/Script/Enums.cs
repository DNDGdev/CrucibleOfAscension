namespace SuperMobileController
{
    //Define enums
    public enum ControlType
    {
        Joystick,
        TargetSkillButton,
        SimpleSkillButton,
        TouchInput,
        CancelArea
    }

    public enum SkillAreaType
    {
        TargetArea,
        TargetDirection
    }

    public enum TouchSelectStatus
    {
        Selected,
        Unselected,
        UnSelectable,
        Selecting,
        Deselecting,
    }
}