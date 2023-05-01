import { Component } from '@angular/core';
import { 
  faTriangleExclamation,
  faComments 
} from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';

interface Currency {
  name: string;
  icon: string;
  address: string;
}

@Component({
  selector: 'app-info',
  templateUrl: './info.component.html',
  styleUrls: ['./info.component.scss']
})
export class InfoComponent {
  faTriangleExclamation = faTriangleExclamation;
  faComments = faComments;
  currentYear = moment().get('year');
  modalCurrency = 'BTC';
  modalVisible = false;
  modalIcon = 'btc.svg';
  modalAddress = '';

  donationCurrencies = {
    btc: {
      name: 'BTC',
      icon: 'btc.svg',
      address: '39yMkox6pP8tnSC7rZ5EM4nUUHgPbg1fKM'
    },
    eth: {
      name: 'ETH',
      icon: 'eth.svg',
      address: '0xc22116Bcf6c30977bEdFcc03C5B6aAe90B0fD179'
    },
    bch: {
      name: 'BCH',
      icon: 'bch.svg',
      address: 'qq02mrtdp454g2zdu534ndpu7jgcr3tvavyzs60m3p'
    }
  }

  openGitHubRepository() {
    window.open('https://github.com/openbullet/OpenBullet2', '_blank');
  }

  openForum() {
    window.open('https://discourse.openbullet.dev', '_blank');
  }

  openBtcDonationModal() {
    this.setModalCurrency(this.donationCurrencies.btc);
    this.modalVisible = true;
  }

  openEthDonationModal() {
    this.setModalCurrency(this.donationCurrencies.eth);
    this.modalVisible = true;
  }

  openBchDonationModal() {
    this.setModalCurrency(this.donationCurrencies.bch);
    this.modalVisible = true;
  }

  private setModalCurrency(currency: Currency) {
    this.modalCurrency = currency.name;
    this.modalIcon = currency.icon;
    this.modalAddress = currency.address;
  }
}
