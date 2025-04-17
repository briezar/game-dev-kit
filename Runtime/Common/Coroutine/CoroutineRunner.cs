using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICoroutineRunner
{
    Coroutine StartCoroutine(IEnumerator enumerator);
    void StopCoroutine(Coroutine coroutine);
}

public class CoroutineRunner : ICoroutineRunner
{
    protected virtual MonoBehaviour runner { get; set; }

    protected CoroutineRunner()
    {

    }

    public CoroutineRunner(MonoBehaviour runner)
    {
        this.runner = runner;
    }

    public Coroutine WaitAndDo(float delay, Action callback)
    {
        return StartCoroutine(WaitAndDoRoutine());
        IEnumerator WaitAndDoRoutine()
        {
            yield return YieldCollection.WaitForSeconds(delay);
            callback?.Invoke();
        }
    }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        if (routine == null) { return null; }
        return runner.StartCoroutine(routine);
    }

    public void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            runner.StopCoroutine(coroutine);
        }
    }

    public void StopAllCoroutines()
    {
        runner.StopAllCoroutines();
    }
}
