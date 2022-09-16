import { Component, EventEmitter, OnInit, Output, Input } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from "ngx-toastr";
import { Router } from '@angular/router';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  model :any = {};
  constructor(private accountService: AccountService, private toastr: ToastrService, private router: Router) { }

  ngOnInit(): void {
  }

  register() {
    this.accountService.register(this.model).subscribe(response => {
      this.router.navigateByUrl('/members');
      this.cancel();
    }, error => {
      console.log(error);
      this.toastr.error(error.error);
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

}
