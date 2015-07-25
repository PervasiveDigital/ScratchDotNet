using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Server.UrlReservations
{
    public class UrlReservation
    {
        private readonly string _url;
        private readonly List<SecurityIdentifier> _securityIdentifiers = new List<SecurityIdentifier>();

        public UrlReservation(string url)
        {
            _url = url;
        }

        public UrlReservation(string url, IList<SecurityIdentifier> securityIdentifiers)
        {
            _url = url;
            _securityIdentifiers.AddRange(securityIdentifiers);
        }

        public string Url
        {
            get { return _url; }
        }

        public ReadOnlyCollection<string> Users
        {
            get
            {
                List<string> users = new List<string>();
                foreach (SecurityIdentifier sec in _securityIdentifiers)
                {
                    users.Add(((NTAccount)sec.Translate(typeof(NTAccount))).Value);
                }
                return new ReadOnlyCollection<string>(users);
            }
        }

        public ReadOnlyCollection<SecurityIdentifier> SIDs
        {
            get
            {
                return new ReadOnlyCollection<SecurityIdentifier>(_securityIdentifiers);
            }
        }

        public void AddUser(string user)
        {
            NTAccount account = new NTAccount(user);
            SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            AddSecurityIdentifier(sid);
        }

        public void AddSecurityIdentifier(SecurityIdentifier sid)
        {
            _securityIdentifiers.Add(sid);
        }

        public void ClearUsers()
        {
            this._securityIdentifiers.Clear();
        }

        public void Create()
        {
            UrlReservationMgr.Create(this);
        }

        public void Delete()
        {
            UrlReservationMgr.Delete(this);
        }
    }
}
