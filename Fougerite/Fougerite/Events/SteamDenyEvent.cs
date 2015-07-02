
using uLink;

namespace Fougerite.Events
{
    public class SteamDenyEvent
    {
        private ClientConnection _cc;
        private NetworkPlayerApproval _approval;
        private string _strReason;
        private NetError _errornum;
        private bool _forceallow = false;

        public SteamDenyEvent(ClientConnection cc, NetworkPlayerApproval approval, string strReason, NetError errornum)
        {
            this._cc = cc;
            this._approval = approval;
            this._strReason = strReason;
            this._errornum = errornum;
        }

        public NetUser NetUser
        {
            get { return _cc.netUser; }
        }

        public ClientConnection ClientConnection
        {
            get { return _cc; }
        }

        public NetworkPlayerApproval Approval
        {
            get { return _approval; }
        }

        public string Reason
        {
            get { return _strReason; }
        }

        public NetError ErrorNumber
        {
            get { return _errornum; }
        }

        public bool ForceAllow
        {
            get { return _forceallow; }
            set { _forceallow = value; }
        }
    }
}
