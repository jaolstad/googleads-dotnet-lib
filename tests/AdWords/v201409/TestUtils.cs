// Copyright 2014, Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Author: api.anash@gmail.com (Anash P. Oommen)

using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.Util.v201409;
using Google.Api.Ads.AdWords.v201409;
using Google.Api.Ads.Common.Util;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Google.Api.Ads.AdWords.Tests.v201409 {
  /// <summary>
  /// A utility class to assist the testing of v201409 services.
  /// </summary>
  class TestUtils {

    public long CreateBudget(AdWordsUser user) {
      BudgetService budgetService =
          (BudgetService) user.GetService(AdWordsService.v201409.BudgetService);

      // Create the campaign budget.
      Budget budget = new Budget();
      budget.name = "Interplanetary Cruise Budget #" + DateTime.Now.ToString(
          "yyyy-M-d H:m:s.ffffff");
      budget.period = BudgetBudgetPeriod.DAILY;
      budget.deliveryMethod = BudgetBudgetDeliveryMethod.STANDARD;
      budget.amount = new Money();
      budget.amount.microAmount = 500000;

      BudgetOperation budgetOperation = new BudgetOperation();
      budgetOperation.@operator = Operator.ADD;
      budgetOperation.operand = budget;

      BudgetReturnValue budgetRetval = budgetService.mutate(new BudgetOperation[] { budgetOperation });
      return budgetRetval.value[0].budgetId;
    }

    /// <summary>
    /// Creates a test search campaign for running further tests.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="biddingStrategy">The bidding strategy to be used.</param>
    /// <returns>The campaign id.</returns>
    public long CreateSearchCampaign(AdWordsUser user, BiddingStrategyType strategyType) {
      return CreateCampaign(user, AdvertisingChannelType.SEARCH, strategyType);
    }

    /// <summary>
    /// Creates a display campaign for running further tests.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="biddingStrategy">The bidding strategy to be used.</param>
    /// <returns>The campaign id.</returns>
    public long CreateDisplayCampaign(AdWordsUser user, BiddingStrategyType strategyType) {
      return CreateCampaign(user, AdvertisingChannelType.DISPLAY, strategyType);
    }

    /// <summary>
    /// Creates a test shopping campaign for running further tests.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="biddingStrategy">The bidding strategy to be used.</param>
    /// <returns>The campaign id.</returns>
    public long CreateShoppingCampaign(AdWordsUser user, BiddingStrategyType strategyType) {
      return CreateCampaign(user, AdvertisingChannelType.SHOPPING, strategyType);
    }

    /// <summary>
    /// Creates a test campaign for running further tests.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="biddingStrategy">The bidding strategy to be used.</param>
    /// <returns>The campaign id.</returns>
    public long CreateCampaign(AdWordsUser user, AdvertisingChannelType channelType,
        BiddingStrategyType strategyType) {
      CampaignService campaignService =
          (CampaignService) user.GetService(AdWordsService.v201409.CampaignService);

      BiddingStrategyConfiguration biddingConfig = new BiddingStrategyConfiguration();
      biddingConfig.biddingStrategyType = strategyType;

      CampaignOperation campaignOperation = new CampaignOperation();
      campaignOperation.@operator = Operator.ADD;
      campaignOperation.operand = new Campaign();
      campaignOperation.operand.name =
          string.Format("Campaign {0}", DateTime.Now.ToString("yyyy-M-d H:m:s.ffffff"));
      campaignOperation.operand.advertisingChannelType = channelType;
      campaignOperation.operand.status = CampaignStatus.PAUSED;
      campaignOperation.operand.biddingStrategyConfiguration = biddingConfig;
      campaignOperation.operand.budget = new Budget();
      campaignOperation.operand.budget.budgetId = CreateBudget(user);
      campaignOperation.operand.budget.period = BudgetBudgetPeriod.DAILY;
      campaignOperation.operand.budget.amount = new Money();
      campaignOperation.operand.budget.amount.microAmount = 100000000;
      campaignOperation.operand.budget.deliveryMethod = BudgetBudgetDeliveryMethod.STANDARD;

      List<Setting> settings = new List<Setting>();

      if (channelType == AdvertisingChannelType.SHOPPING) {
        // All Shopping campaigns need a ShoppingSetting.
        ShoppingSetting shoppingSetting = new ShoppingSetting();
        shoppingSetting.salesCountry = "US";
        shoppingSetting.campaignPriority = 0;
        shoppingSetting.merchantId = (user.Config as AdWordsAppConfig).MerchantCenterId;

        settings.Add(shoppingSetting);
      }
      campaignOperation.operand.settings = settings.ToArray();

      CampaignReturnValue retVal =
          campaignService.mutate(new CampaignOperation[] {campaignOperation});
      return retVal.value[0].id;
    }

    /// <summary>
    /// Creates a test adgroup for running further tests.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="campaignId">The campaign id for which the adgroup is created.</param>
    /// <returns>The adgroup id.</returns>
    public long CreateAdGroup(AdWordsUser user, long campaignId) {
      return CreateAdGroup(user, campaignId, false);
    }

    /// <summary>
    /// Creates a test adgroup for running further tests.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="campaignId">The campaign id for which the adgroup is created.</param>
    /// <param name="isCpmBid">True, if a ManualCPM bid is to be used.</param>
    /// <returns>The adgroup id.</returns>
    public long CreateAdGroup(AdWordsUser user, long campaignId, bool isCpmBid) {
      AdGroupService adGroupService =
          (AdGroupService) user.GetService(AdWordsService.v201409.AdGroupService);

      AdGroupOperation adGroupOperation = new AdGroupOperation();
      adGroupOperation.@operator = Operator.ADD;
      adGroupOperation.operand = new AdGroup();
      adGroupOperation.operand.campaignId = campaignId;
      adGroupOperation.operand.name =
          string.Format("AdGroup {0}", DateTime.Now.ToString("yyyy-M-d H:m:s.ffffff"));
      adGroupOperation.operand.status = AdGroupStatus.ENABLED;

      if (isCpmBid) {
        BiddingStrategyConfiguration biddingConfig = new BiddingStrategyConfiguration();
        CpmBid cpmBid = new CpmBid();
        cpmBid.bid = new Money();
        cpmBid.bid.microAmount = 10000000;
        biddingConfig.bids = new Bids[] {cpmBid};
        adGroupOperation.operand.biddingStrategyConfiguration = biddingConfig;
      } else {
        BiddingStrategyConfiguration biddingConfig = new BiddingStrategyConfiguration();
        CpcBid cpcBid = new CpcBid();
        cpcBid.bid = new Money();
        cpcBid.bid.microAmount = 10000000;
        biddingConfig.bids = new Bids[] {cpcBid};
        adGroupOperation.operand.biddingStrategyConfiguration = biddingConfig;
      }
      AdGroupReturnValue retVal = adGroupService.mutate(new AdGroupOperation[] {adGroupOperation});
      return retVal.value[0].id;
    }

    /// <summary>
    /// Creates a test textad for running further tests.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="adGroupId">The adgroup id for which the ad is created.
    /// </param>
    /// <param name="hasAdParam">True, if an ad param placeholder should be
    /// added.</param>
    /// <returns>The text ad id.</returns>
    public long CreateTextAd(AdWordsUser user, long adGroupId, bool hasAdParam) {
      AdGroupAdService adGroupAdService =
          (AdGroupAdService) user.GetService(AdWordsService.v201409.AdGroupAdService);
      AdGroupAdOperation adGroupAdOperation = new AdGroupAdOperation();
      adGroupAdOperation.@operator = Operator.ADD;
      adGroupAdOperation.operand = new AdGroupAd();
      adGroupAdOperation.operand.adGroupId = adGroupId;
      TextAd ad = new TextAd();

      ad.headline = "Luxury Cruise to Mars";
      ad.description1 = "Visit the Red Planet in style.";
      if (hasAdParam) {
        ad.description2 = "Low-gravity fun for {param1:cheap}!";
      } else {
        ad.description2 = "Low-gravity fun for everyone!";
      }
      ad.displayUrl = "example.com";
      ad.url = "http://www.example.com";

      adGroupAdOperation.operand.ad = ad;

      AdGroupAdReturnValue retVal =
          adGroupAdService.mutate(new AdGroupAdOperation[] {adGroupAdOperation});
      return retVal.value[0].ad.id;
    }

    /// <summary>
    /// Creates a test ThirdPartyRedirectAd for running further tests.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="adGroupId">The adgroup id for which the ad is created.
    /// </param>
    /// <param name="hasAdParam">True, if an ad param placeholder should be
    /// added.</param>
    /// <returns>The text ad id.</returns>
    public long CreateThirdPartyRedirectAd(AdWordsUser user, long adGroupId) {
      AdGroupAdService adGroupAdService =
          (AdGroupAdService) user.GetService(AdWordsService.v201409.AdGroupAdService);
      AdGroupAdOperation adGroupAdOperation = new AdGroupAdOperation();
      adGroupAdOperation.@operator = Operator.ADD;
      adGroupAdOperation.operand = new AdGroupAd();
      adGroupAdOperation.operand.adGroupId = adGroupId;

      // Create the third party redirect ad.
      ThirdPartyRedirectAd redirectAd = new ThirdPartyRedirectAd();
      redirectAd.name = string.Format("Example third party ad #{0}", this.GetTimeStamp());
      redirectAd.url = "http://www.example.com";

      redirectAd.dimensions = new Dimensions();
      redirectAd.dimensions.height = 250;
      redirectAd.dimensions.width = 300;

      // This field normally contains the javascript ad tag.
      redirectAd.snippet =
          "<img src=\"http://www.google.com/intl/en/adwords/select/images/samples/inline.jpg\"/>";
      redirectAd.impressionBeaconUrl = "http://www.examples.com/beacon";
      redirectAd.certifiedVendorFormatId = 119;
      redirectAd.isCookieTargeted = false;
      redirectAd.isUserInterestTargeted = false;
      redirectAd.isTagged = false;

      adGroupAdOperation.operand.ad = redirectAd;

      AdGroupAdReturnValue retVal =
          adGroupAdService.mutate(new AdGroupAdOperation[] {adGroupAdOperation});
      return retVal.value[0].ad.id;
    }

    /// <summary>
    /// Sets an adparam for running further tests.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="adGroupId">The adgroup id to which criterionId belongs.
    /// </param>
    /// <param name="criterionId">The criterion id to which adparam is set.
    /// </param>
    public void SetAdParam(AdWordsUser user, long adGroupId, long criterionId) {
      AdParamService adParamService =
          (AdParamService) user.GetService(AdWordsService.v201409.AdParamService);

      // Prepare for setting ad parameters.
      AdParam adParam = new AdParam();
      adParam.adGroupId = adGroupId;
      adParam.criterionId = criterionId;
      adParam.paramIndex = 1;
      adParam.insertionText = "$100";

      AdParamOperation adParamOperation = new AdParamOperation();
      adParamOperation.@operator = Operator.SET;
      adParamOperation.operand = adParam;

      // Set ad parameters.
      AdParam[] newAdParams = adParamService.mutate(new AdParamOperation[] {adParamOperation});
      return;
    }

    /// <summary>
    /// Creates a keyword for running further tests.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="adGroupId">The adgroup id for which the keyword is
    /// created.</param>
    /// <returns>The keyword id.</returns>
    public long CreateKeyword(AdWordsUser user, long adGroupId) {
      AdGroupCriterionService adGroupCriterionService =
         (AdGroupCriterionService) user.GetService(AdWordsService.v201409.AdGroupCriterionService);

      AdGroupCriterionOperation operation = new AdGroupCriterionOperation();
      operation.@operator = Operator.ADD;
      operation.operand = new BiddableAdGroupCriterion();
      operation.operand.adGroupId = adGroupId;

      Keyword keyword = new Keyword();
      keyword.matchType = KeywordMatchType.BROAD;
      keyword.text = "mars cruise";

      operation.operand.criterion = keyword;
      AdGroupCriterionReturnValue retVal =
          adGroupCriterionService.mutate(new AdGroupCriterionOperation[] {operation});
      return retVal.value[0].criterion.id;
    }

    /// <summary>
    /// Creates the placement.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="adGroupId">The adgroup id for which the placement is
    /// created.</param>
    /// <returns>The placement id.</returns>
    public long CreatePlacement(AdWordsUser user, long adGroupId) {
      AdGroupCriterionService adGroupCriterionService =
          (AdGroupCriterionService) user.GetService(AdWordsService.v201409.AdGroupCriterionService);

      Placement placement = new Placement();
      placement.url = "http://mars.google.com";

      AdGroupCriterion placementCriterion = new BiddableAdGroupCriterion();
      placementCriterion.adGroupId = adGroupId;
      placementCriterion.criterion = placement;

      AdGroupCriterionOperation placementOperation = new AdGroupCriterionOperation();
      placementOperation.@operator = Operator.ADD;
      placementOperation.operand = placementCriterion;

      AdGroupCriterionReturnValue retVal = adGroupCriterionService.mutate(
          new AdGroupCriterionOperation[] {placementOperation});

      return retVal.value[0].criterion.id;
    }

    /// <summary>
    /// Adds an experiment.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="campaignId">The campaign id.</param>
    /// <param name="adGroupId">The ad group id.</param>
    /// <param name="criterionId">The criterion id.</param>
    /// <returns>The experiment id.</returns>
    public long AddExperiment(AdWordsUser user, long campaignId, long adGroupId, long criterionId) {
      // Get the ExperimentService.
      ExperimentService experimentService =
          (ExperimentService) user.GetService(AdWordsService.v201409.ExperimentService);

      // Get the AdGroupService.
      AdGroupService adGroupService =
          (AdGroupService) user.GetService(AdWordsService.v201409.AdGroupService);

      // Get the AdGroupCriterionService.
      AdGroupCriterionService adGroupCriterionService =
          (AdGroupCriterionService) user.GetService(AdWordsService.v201409.AdGroupCriterionService);

      // Create the experiment.
      Experiment experiment = new Experiment();
      experiment.campaignId = campaignId;
      experiment.name = "Interplanetary Cruise #" + GetTimeStamp();
      experiment.queryPercentage = 10;
      experiment.startDateTime = DateTime.Now.AddDays(1).ToString("yyyyMMdd HHmmss");

      // Create the operation.
      ExperimentOperation experimentOperation = new ExperimentOperation();
      experimentOperation.@operator = Operator.ADD;
      experimentOperation.operand = experiment;

      // Add the experiment.
      ExperimentReturnValue experimentRetVal = experimentService.mutate(
          new ExperimentOperation[] {experimentOperation});

      return experimentRetVal.value[0].id;
    }

    /// <summary>
    /// Adds the campaign targeting criteria to a campaign.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="campaignId">The campaign id.</param>
    /// <returns>The campaign criteria id.</returns>
    public long AddCampaignTargetingCriteria(AdWordsUser user, long campaignId) {
      // Get the CampaignCriterionService.
      CampaignCriterionService campaignCriterionService =
          (CampaignCriterionService) user.GetService(
              AdWordsService.v201409.CampaignCriterionService);

      // Create language criteria.
      // See http://code.google.com/apis/adwords/docs/appendix/languagecodes.html
      // for a detailed list of language codes.
      Language language1 = new Language();
      language1.id = 1002; // French
      CampaignCriterion languageCriterion1 = new CampaignCriterion();
      languageCriterion1.campaignId = campaignId;
      languageCriterion1.criterion = language1;

      CampaignCriterion[] criteria = new CampaignCriterion[] {languageCriterion1};

      List<CampaignCriterionOperation> operations = new List<CampaignCriterionOperation>();

      foreach (CampaignCriterion criterion in criteria) {
        CampaignCriterionOperation operation = new CampaignCriterionOperation();
        operation.@operator = Operator.ADD;
        operation.operand = criterion;
        operations.Add(operation);
      }

      CampaignCriterionReturnValue retVal = campaignCriterionService.mutate(operations.ToArray());
      return retVal.value[0].criterion.id;
    }

    /// <summary>
    /// Returns an image which can be used for creating image ads.
    /// </summary>
    /// <returns>The image data, as an array of bytes.</returns>
    public byte[] GetTestImage() {
      return MediaUtilities.GetAssetDataFromUrl("http://goo.gl/HJM3L");
    }

    /// <summary>
    /// Gets the geo location for a given address.
    /// </summary>
    /// <param name="user">The AdWords user.</param>
    /// <param name="address">The address for which geolocation should be
    /// fetched.</param>
    /// <returns>Geo location for the address.</returns>
    public GeoLocation GetLocationForAddress(AdWordsUser user, Address address) {
      GeoLocationService geoService =
          (GeoLocationService) user.GetService(AdWordsService.v201409.GeoLocationService);

      GeoLocationSelector selector = new GeoLocationSelector();
      selector.addresses = new Address[] {address};
      return geoService.get(selector)[0];
    }

    /// <summary>
    /// Gets the current timestamp.
    /// </summary>
    /// <returns>The timestamp as a string.</returns>
    public string GetTimeStamp() {
      return (DateTime.UtcNow - new DateTime(1970, 1, 1)).
          TotalMilliseconds.ToString();
    }

    /// <summary>
    /// Gets the current timestamp as an alphabetic string.
    /// </summary>
    /// <returns>The timestamp as a string.</returns>
    public string GetTimeStampAlpha() {
      string timeStamp = GetTimeStamp();
      StringBuilder builder = new StringBuilder();
      for (int i = 0; i < timeStamp.Length; i++) {
        if (timeStamp[i] == '.') {
          continue;
        }
        builder.Append(Convert.ToChar('a' + int.Parse(timeStamp[i].ToString())));
      }
      return builder.ToString();
    }
  }
}
