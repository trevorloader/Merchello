﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Merchello.Core;
using Merchello.Core.Models;
using Merchello.Core.Services;
using Merchello.Web.WebApi;
using Merchello.Web.Models.ContentEditing;
using System.Net;
using System.Net.Http;

namespace Merchello.Web.Editors
{
    [PluginController("Merchello")]
    public class SettingsApiController : MerchelloApiController
    {
        private readonly IStoreSettingService _storeSettingService;

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsApiController()
            : this(MerchelloContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="merchelloContext"></param>
        public SettingsApiController(MerchelloContext merchelloContext)
            : base(merchelloContext)
        {
            _storeSettingService = MerchelloContext.Services.StoreSettingService;
        }

        /// <summary>
        /// This is a helper contructor for unit testing
        /// </summary>
        internal SettingsApiController(MerchelloContext merchelloContext, UmbracoContext umbracoContext)
            : base(merchelloContext, umbracoContext)
        {
            _storeSettingService = MerchelloContext.Services.StoreSettingService;
        }

        /// <summary>
        /// Returns Country for the countryCode passed in
        /// 
        /// GET /umbraco/Merchello/SettingsApi/GetCountry/{countryCode}
        /// </summary>
        /// <param name="id">Country code to get</param>
        public CountryDisplay GetCountry(string id)
        {
            ICountry country = _storeSettingService.GetCountryByCode(id);
            if (country == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return country.ToCountryDisplay();
        }

        /// <summary>
        /// Returns All Countries
        /// 
        /// GET /umbraco/Merchello/SettingsApi/GetAllCountries
        /// </summary>
        public IEnumerable<CountryDisplay> GetAllCountries()
        {
            var countries = _storeSettingService.GetAllCountries();
            if (countries == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            foreach (ICountry country in countries)
            {
                yield return country.ToCountryDisplay();
            }
        }

        /// <summary>
        /// Returns All Countries with a list of country codes to exclude
        /// 
        /// GET /umbraco/Merchello/SettingsApi/GetAllCountriesExcludeCodes?codes={string}&codes={string}
        /// </summary>
        /// <param name="codes">Country codes to exclude</param>
        public IEnumerable<CountryDisplay> GetAllCountriesExcludeCodes([FromUri]string[] codes)
        {
            var countries = _storeSettingService.GetAllCountries(codes);
            if (countries == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            foreach (ICountry country in countries)
            {
                yield return country.ToCountryDisplay();
            }
        }

        /// <summary>
        /// Returns All Tax Provinces
        /// 
        /// GET /umbraco/Merchello/SettingsApi/GetAllTaxProvinces
        /// </summary>
        public IEnumerable<TaxMethodDisplay> GetAllTaxProvinces()
        {
            // TODO: replace with call to service
            var taxMethods = new List<TaxMethod>();

            //var oregon = new TaxCountry("OR", "Oregon");
            //oregon.Rate = 0.01M;
            //taxProvinces.Add(oregon);

            //var washington = new TaxCountry("WA", "Washington");
            //washington.Rate = 0.09M;
            //taxProvinces.Add(washington);

            // END TEST DATA

            foreach (TaxMethod taxMethod in taxMethods)
            {
                yield return taxMethod.ToTaxMethodDisplay();
            }
        }

		/// <summary>
		/// Returns All Tax Provinces
		/// 
		/// GET /umbraco/Merchello/SettingsApi/GetAllTaxProvinces
		/// </summary>
        public IEnumerable<ICurrency> GetAllCurrencies()
		{
			// TODO: replace with call to service
            var currencyList = _storeSettingService.GetAllCurrencies();

		    if (currencyList == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
		    }

            return currencyList;
		}

		/// <summary>
		/// Returns Product by id (key) 
		/// GET /umbraco/Merchello/ProductApi/GetProduct/{guid}
		/// </summary>
		/// <param name="id"></param>
		public SettingDisplay GetAllSettings()
		{																								   
			var settings = _storeSettingService.GetAll();
			var settingDisplay = new SettingDisplay();

			if (settings == null)
			{
				throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
			}

			return settingDisplay.ToStoreSettingDisplay(settings);
		}

        /// <summary>
        /// Gets the nextInvoiceNumber and nextOrderNumber
        /// </summary>
        /// <returns>Next Invoice Number and Next Order Number</returns>
        public SettingDisplay GetInvoiceAndOrderNumbers()
        {
            var settingDisplay = new SettingDisplay
            {
                nextInvoiceNumber = _storeSettingService.GetNextInvoiceNumber(),
                nextOrderNumber = _storeSettingService.GetNextOrderNumber()
            };
            
            return settingDisplay;
        }

		/// <summary>
		/// Updates existing global settings
		///
		/// PUT /umbraco/Merchello/ProductApi/PutSettings
		/// </summary>
		/// <param name="setting">SettingDisplay object serialized from WebApi</param>
		[AcceptVerbs("POST", "PUT")]
		public HttpResponseMessage PutSettings(SettingDisplay setting)
		{
			var response = Request.CreateResponse(HttpStatusCode.OK);

			try
			{
				IEnumerable<IStoreSetting> merchSetting = setting.ToStoreSetting(_storeSettingService.GetAll());
				foreach(var s in merchSetting)
				{
					_storeSettingService.Save(s);
				}
			}
			catch (Exception ex)
			{
				response = Request.CreateResponse(HttpStatusCode.NotFound, String.Format("{0}", ex.Message));
			}

			return response;
		}
    }
}
