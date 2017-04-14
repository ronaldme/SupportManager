using System;
using System.IO.Ports;
using MYCroes.ATCommands;
using MYCroes.ATCommands.Forwarding;

namespace SupportManager.Control
{
    public class ATHelper : IDisposable
    {
        private readonly SerialPort serialPort;

        public ATHelper(string port)
        {
            serialPort = new SerialPort(port);
            serialPort.Open();
        }

        public void ForwardTo(string phoneNumberWithInternationalAccessCode)
        {
            var cmd = ForwardingStatus.Set(ForwardingReason.Unconditional, ForwardingMode.Registration,
                phoneNumberWithInternationalAccessCode, ForwardingPhoneNumberType.WithInternationalAccessCode,
                ForwardingClass.Voice);

            Execute(cmd);
        }

        public string GetForwardedPhoneNumber()
        {
            var cmd = ForwardingStatus.Query(ForwardingReason.Unconditional);
            var res = Execute(cmd);

            return ForwardingStatus.Parse(res[0]).Number;
        }

        private string[] Execute(ATCommand command)
        {
            var stream = serialPort.BaseStream;
            return command.Execute(stream);
        }

        public void Dispose()
        {
            if (serialPort.IsOpen) serialPort.Close();

            serialPort.Dispose();
        }
    }
}