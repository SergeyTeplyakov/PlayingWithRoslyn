//using System;
//using System.Diagnostics.Contracts;

//namespace SampleNuGetAnalyzer.Commands
//{
//    public struct RelayCommand
//    {
//        private readonly Action _action;

//        public RelayCommand(Action action)
//        {
//            Contract.Requires(action != null);
//            _action = action;
//        }

//        public void Run()
//        {
//            _action();
//        }
//    }
//}