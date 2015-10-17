using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmUltraLight.Core
{
public struct RelayCommand
{
    private readonly Action _action;

    public RelayCommand(Action action)
    {
        Contract.Requires(action != null);
        _action = action;
    }

    public void Run()
    {
        _action();
    }
}
}
