using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UserType { None, Doctor, Patient }

public static class AppState
{
    public static UserType CurrentUser = UserType.None;
}
