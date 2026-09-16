using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeliverySimulator
{
    public partial class Form1 : Form
    {
        private readonly List<DeliveryMan> _deliveryMen;
        private readonly Warehouse _warehouse;
        private readonly Supplier _supplier;
        private readonly Dictionary<int, Label> _deliveryLabels = new Dictionary<int, Label>();
        private readonly Timer _timer;
        private void CreateDeliveryLabels()
        {
            foreach (DeliveryMan deliveryMan in _deliveryMen)
            {
                Label label = new Label();

                label.Text = $"🚚 {deliveryMan.Id + 1}";
                label.AutoSize = true;
                label.Font = new Font("Segoe UI Emoji", 8);

                label.Location = deliveryMan.Position;

                Controls.Add(label);

                _deliveryLabels.Add(deliveryMan.Id, label);
            }
        }
        internal Form1(Warehouse warehouse, Supplier supplier, List<DeliveryMan> deliveryMen)
        {
            InitializeComponent();
            packagesLabel.Text = warehouse.PackageCount.ToString();
            _warehouse = warehouse;
            _supplier = supplier;
            _deliveryMen = deliveryMen;
            CreateDeliveryLabels();
            _timer = new Timer();
            _timer.Interval = 100;
            _timer.Tick += UpdateUI;
            _timer.Start();
        }
        private void UpdateUI(object sender, EventArgs e)
        {
            packagesLabel.Text = $"Packages in the warehouse: {_warehouse.PackageCount}";

            foreach (DeliveryMan deliveryMan in _deliveryMen)
            {
                Label label = _deliveryLabels[deliveryMan.Id];

                label.Location = deliveryMan.Position;
            }
        }
    }
}
