import { Component, inject, OnInit } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/member';

@Component({
  selector: 'app-member-edit',
  imports: [],
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css'
})
export class MemberEditComponent implements OnInit {
  private accountservice = inject(AccountService);
  private memberService = inject(MembersService);
  member? : Member;

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember(){
    const user = this.accountservice.currentUser();
    if(!user) return;
    this.memberService.getMember(user.userName).subscribe({
      next: member => this.member = member
    })
  }
}
