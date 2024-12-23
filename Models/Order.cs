using System;
using TBLApi.Models;
namespace TBPApi
{
    public class Order
    {
        public int Id { get; set; } // ��������� ����
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string ServiceType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ClientName { get; set; }  // ��� �������
        public string ClientPhoto { get; set; }
    }
}
