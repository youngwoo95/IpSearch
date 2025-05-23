﻿using System.ComponentModel.DataAnnotations;

namespace IpManager.DTO.Store
{
    public class UpdateStoreDTO
    {
        /// <summary>
        /// PID
        /// </summary>
        [Required]
        public int pId { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        [Required]
        public string ip { get; set; } = null!;

        /// <summary>
        /// 포트
        /// </summary>
        [Required]
        public int port { get; set; }

        /// <summary>
        /// 상호명
        /// </summary>
        [Required]
        public string name { get; set; } = null!;

        /// <summary>
        /// 좌석수
        /// </summary>
        [Required]
        public int seatNumber { get; set; }

        /// <summary>
        /// 요금제
        /// </summary>
        [Required]
        public float price { get; set; }

        /// <summary>
        /// 요금제 비율
        /// </summary>
        [Required]
        public float pricePercent { get; set; }

        /// <summary>
        /// PC 사양
        /// </summary>
        public string? pcSpec { get; set; }

        /// <summary>
        /// 통신사
        /// </summary>
        public string? telecom { get; set; }

        /// <summary>
        /// 메모
        /// </summary>
        public string? memo { get; set; }

   

    }
}
