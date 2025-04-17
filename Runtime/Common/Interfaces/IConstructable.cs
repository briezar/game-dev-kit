using System;
using System.Collections;
using System.Collections.Generic;

public interface IConstructable { void Construct(); }
public interface IConstructable<T> { void Construct(T args); }
public interface IConstructable<T1, T2> { void Construct(T1 arg1, T2 arg2); }
public interface IConstructable<T1, T2, T3> { void Construct(T1 arg1, T2 arg2, T3 arg3); }

public class ConstructorArgs
{
    public static readonly ConstructorArgs Empty = new();
}