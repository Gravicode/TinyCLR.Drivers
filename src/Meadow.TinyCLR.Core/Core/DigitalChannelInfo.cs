using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Meadow.TinyCLR.Core
{
    public interface IChannelInfo
    {
        string Name { get; }
    }
    public interface IDigitalChannelInfo : IChannelInfo
    {
        bool InputCapable { get; }

        bool OutputCapable { get; }

        bool InterruptCapable { get; }

        bool PullDownCapable { get; }

        bool PullUpCapable { get; }

        bool InverseLogic { get; }
    }
    public abstract class ChannelInfoBase
    {
        public string Name { get; protected set; }

        protected ChannelInfoBase(string name) => this.Name = name;
    }
    public class DigitalChannelInfoBase : ChannelInfoBase, IDigitalChannelInfo, IChannelInfo
    {
        public bool InputCapable { get; protected set; }

        public bool OutputCapable { get; protected set; }

        public bool InterruptCapable { get; protected set; }

        public bool PullDownCapable { get; protected set; }

        public bool PullUpCapable { get; protected set; }

        public bool InverseLogic { get; protected set; }

        protected DigitalChannelInfoBase(
          string name,
          bool inputCapable,
          bool outputCapable,
          bool interruptCapable,
          bool pullDownCapable,
          bool pullUpCapable,
          bool inverseLogic)
          : base(name)
        {
            this.InputCapable = inputCapable;
            this.OutputCapable = outputCapable;
            this.InterruptCapable = interruptCapable;
            this.PullDownCapable = pullDownCapable;
            this.PullUpCapable = pullUpCapable;
            this.InverseLogic = inverseLogic;
        }
    }
    public class DigitalChannelInfo : DigitalChannelInfoBase
    {
        public DigitalChannelInfo(
          string name,
          bool inputCapable = true,
          bool outputCapable = true,
          bool interruptCapable = true,
          bool pullDownCapable = true,
          bool pullUpCapable = true,
          bool inverseLogic = false)
          : base(name, inputCapable, outputCapable, interruptCapable, pullDownCapable, pullUpCapable, inverseLogic)
        {
        }
    }
}
