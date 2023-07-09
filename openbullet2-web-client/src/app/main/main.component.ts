import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss']
})
export class MainComponent implements OnInit {
  constructor(private router: Router) {

  }

  ngOnInit(): void {
    if (window.location.pathname === '/') {
      this.router.navigate(['home']);
    }
  }
}
