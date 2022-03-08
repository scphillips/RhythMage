// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, March 2022

using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    public event System.Action OnUpdate;
    
    void Update()
    {
        OnUpdate?.Invoke();
    }
}
