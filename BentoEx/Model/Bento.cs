using BentoEx.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BentoEx.Model
{
    public class Bento : BindableBase
    {
        // These should be properties
        public DateTime BentoDate { get; set; }
        public string BentoMenuStr
        {
            get
            {
                return BentoMenu + GetBentoTypeStr(Type);
            }
        }
        public string BentoMenu { get; set; }

        private int price;
        public int Price
        {
            get { return price; }
            set
            {
                price = value;
                PriceStr = $"{price}円";
            }
        }
        public string PriceStr { get; private set; }
        
        public enum BentoType
        {
            normal,
            ohmori,
            okazu
        }
        public BentoType Type { get; set; }

        private bool toBeOrdered;
        public bool ToBeOrdered
        {
            get
            {
                return toBeOrdered;
            }
            set
            {
                toBeOrdered = value;
                NotifyPropertyChanged();
            }
        }

        public enum OrderStatus
        {
            blank,
            ordered,
            expired
        }
        private OrderStatus orderState;
        public OrderStatus OrderState
        {
            get { return orderState; }
            set
            {
                orderState = value;
                canOrder = (orderState == OrderStatus.blank) ? true : false;
                NotifyPropertyChanged();
            }
        }

        private bool canOrder;
        public bool CanOrder
        {
            get { return canOrder; }
            set
            {
                canOrder = value;
                NotifyPropertyChanged();
            }
        }

        private string GetBentoTypeStr(BentoType type)
        {
            if (type == BentoType.ohmori)
                return " (ライス大盛)";
            else if (type == BentoType.okazu)
                return " (おかずのみ)";
            else
                return "";
        }
    }
}
