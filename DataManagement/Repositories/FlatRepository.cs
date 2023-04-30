﻿using Application.Interfaces.Repository;
using DataManagement.Models;
using WebScraper.Models;

namespace DataManagement.Repositories
{
    public class FlatRepository : IFlatRepository
    {
        private readonly RentFinderDbContext _context;

        public FlatRepository(RentFinderDbContext context)
        {
            _context = context;
        }

        public void CreateFlats(List<FlatInfoModel> flatInfoModels)
        {
            foreach (var flat in flatInfoModels)
            {
                CreateFlat(flat);
            }
        }

        private void CreateFlat(FlatInfoModel flatInfoModel)
        {
            var phoneId = CreateOrUpgradePhoneNumberAndGetHisId(flatInfoModel.PhoneNumber);

            var flatInfoId = CreateFlatInfoAndGetHisId(flatInfoModel, phoneId);

            CreateFlatImages(flatInfoModel.LinksOfImages, flatInfoId);

            _context.FlatDateInfosDto.Add(new FlatDateInfoDto()
            { FlatInfoId = flatInfoId, SitePublication = flatInfoModel.SitePublication.ToUniversalTime() });
            _context.SaveChanges();
        }

        private long CreateFlatInfoAndGetHisId(FlatInfoModel flatInfoModel, long phoneId)
        {
            var flatDto = new FlatInfoDto()
            {
                Title = flatInfoModel.Title,
                Cost = flatInfoModel.Cost,
                Description = flatInfoModel.Description,
                ViewsOnSite = flatInfoModel.ViewsOnSite,
                PageLink = flatInfoModel.PageLink,
                FlatPhoneId = phoneId
            };

            _context.FlatInfosDto.Add(flatDto);
            _context.SaveChanges();

            return flatDto.Id;
        }


        private void CreateFlatImages(List<string> imageLinks, long flatId)
        {
            imageLinks.ForEach(link =>
            {
                _context.FlatLinksImage.Add(new FlatLinkImage() { FlatInfoId = flatId, Link = link });
                _context.SaveChanges();
            });
        }

        private long CreateOrUpgradePhoneNumberAndGetHisId(string number)
        {
            var phoneModel = _context.FlatPhonesDto.SingleOrDefault(p => p.Number == number);

            if (phoneModel == null)
            {
                var flatPhone = new FlatPhoneDto() { Number = number, NumberMentionsOnSite = 1 };
                _context.FlatPhonesDto.Add(flatPhone);
                _context.SaveChanges();
                return flatPhone.Id;
            }

            phoneModel.NumberMentionsOnSite += 1;
            _context.FlatPhonesDto.Update(phoneModel);
            _context.SaveChanges();

            return phoneModel.Id;
        }
    }
}
