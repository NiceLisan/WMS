﻿using Coldairarrow.Entity.IT;
using Coldairarrow.Entity.PB;
using Coldairarrow.Entity.TD;
using Coldairarrow.Util;
using EFCore.Sharding;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Coldairarrow.Business.IT
{
    public partial class IT_LocalMaterialBusiness
    {
        public async Task<List<IT_LocalMaterial>> LoadCheckDataByAreaIdAsync(string id)
        {
            var q = GetIQueryable();
            q = q.Include(i => i.Location).Include(i => i.Material);

            q = q.Where(p => p.Location.AreaId == id);

            return await q.ToListAsync();

        }

        public async Task<List<IT_LocalMaterial>> LoadCheckDataByMaterialAsync(string storId, List<string> ids)
        {
            var q = GetIQueryable();
            q = q.Include(i => i.Location).Include(i => i.Material);

            q = q.Where(p => p.StorId == storId && ids.Contains(p.MaterialId));

            return await q.ToListAsync();
        }

        public async Task<List<PB_Material>> LoadMaterialByRandomAsync(string storId, int per)
        {
            var q = GetIQueryable();
            q = q.Include(i => i.Material).Include(i => i.Tray);

            q = q.Where(p => p.StorId == storId);

            var total = await q.Select(u => u.Material).Distinct().ToListAsync();
            var num = total.Count;
            var tackNum = Convert.ToInt32(num * (Convert.ToDouble(per) / 100.0));
            var tackList = new List<string>();
            Random rnd = new Random();

            int idx = 0;
            while (idx < tackNum)
            {
                var randomIdx = rnd.Next(0, num - 1);
                var item = total[randomIdx].Id;
                if (!tackList.Contains(item))
                {
                    tackList.Add(item);
                    idx += 1;
                }
            }

            return (from u in total where tackList.Contains(u.Id) select u).ToList();
        }

        public async Task AddDataAsync(List<IT_LocalMaterial> list)
        {
            await InsertAsync(list);
        }

        public async Task UpdateDataAsync(List<IT_LocalMaterial> list)
        {
            await UpdateAsync(list);
        }

        public async Task UpdataDatasByBussiness(List<BusinessInfo> list)
        {
            if (list.Count > 0)
            {
                var q = GetIQueryable();
                var StorIdList = list.Select(i => i.StorId).Distinct().ToList();
                var LocalIdList = list.Select(i => i.LocalId).Distinct().ToList();
                var TrayIdList = list.Select(i => i.TrayId).Distinct().ToList();
                var ZoneIdList = list.Select(i => i.ZoneId).Distinct().ToList();
                var MaterialIdList = list.Select(i => i.MaterialId).Distinct().ToList();
                var BatchNoList = list.Select(i => i.BatchNo).Distinct().ToList();
                var BarCodeList = list.Select(i => i.BarCode).Distinct().ToList();
                var hasMatch = 0;

                q = q.Where(w => StorIdList.Contains(w.StorId) && LocalIdList.Contains(w.LocalId) && TrayIdList.Contains(w.TrayId)
                && ZoneIdList.Contains(w.ZoneId) && MaterialIdList.Contains(w.MaterialId) && BatchNoList.Contains(w.BatchNo)
                && BarCodeList.Contains(w.BarCode));

                var datalist = await q.ToListAsync();
                var addDatas = new List<IT_LocalMaterial>();
                var updataDatas = new List<IT_LocalMaterial>();

                foreach (var b in list)
                {
                    hasMatch = 0;
                    foreach (var i in datalist)
                    {
                        if (b.StorId == i.StorId && b.LocalId == i.LocalId && b.TrayId == i.TrayId && b.ZoneId == i.ZoneId
                             && b.MaterialId == i.MaterialId && b.BatchNo == i.BatchNo && b.BarCode == i.BarCode)
                        {
                            if (b.ActionType == 1)
                            {
                                i.Num -= b.Num;
                            }
                            else
                            {
                                i.Num += b.Num;
                            }
                            updataDatas.Add(i);
                            hasMatch = 1;
                            break;
                        }
                    }
                    if (hasMatch == 0)
                    {
                        addDatas.Add(new IT_LocalMaterial()
                        {
                            Id = IdHelper.GetId(),
                            StorId = b.StorId,
                            LocalId = b.LocalId,
                            TrayId = b.TrayId,
                            ZoneId = b.ZoneId,
                            MaterialId = b.MaterialId,
                            BarCode = b.BarCode,
                            BatchNo = b.BatchNo,
                            Num = b.Num,
                            MeasureId = b.MeasureId
                        });
                    }
                }

                if (updataDatas.Count > 0)
                {
                    await UpdateAsync(updataDatas);
                }

                if (addDatas.Count > 0)
                {
                    await InsertAsync(addDatas);
                }
            }
        }

        public async Task<IT_LocalMaterial> GetDataByBussiness(BusinessInfo businessInfo)
        {

            var q = GetIQueryable();

            q = q.Where(w => w.StorId == businessInfo.StorId && w.LocalId == businessInfo.LocalId && w.TrayId == businessInfo.TrayId
                && w.ZoneId == businessInfo.ZoneId && w.MaterialId == businessInfo.MaterialId && w.BatchNo == businessInfo.BatchNo
                && w.BarCode == businessInfo.BarCode);

            var datalist = await q.ToListAsync();

            foreach (var i in datalist)
            {
                if (businessInfo.StorId == i.StorId && businessInfo.LocalId == i.LocalId && businessInfo.TrayId == i.TrayId && businessInfo.ZoneId == i.ZoneId
                     && businessInfo.MaterialId == i.MaterialId && businessInfo.BatchNo == i.BatchNo && businessInfo.BarCode == i.BarCode)
                {
                    return i;
                }
            }

            return null;
        }
    }
}