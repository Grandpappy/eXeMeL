using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eXeMeL.ViewModel;

namespace eXeMeL.Messages
{
  public class ActivateSnapshotMessage
  {
    public DocumentSnapshot Snapshot { get; set; }


    public ActivateSnapshotMessage(DocumentSnapshot snapshot)
    {
      this.Snapshot = snapshot;
    }
  }
}
