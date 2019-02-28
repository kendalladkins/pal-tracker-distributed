create table claims (
  id         bigint(20) not null auto_increment,
  user_id bigint(20),
  project_id bigint(20),
  claim_type VARCHAR(32),
  expense_total decimal,
  expense_date datetime,
  primary key (id)
)
engine = innodb
default charset = utf8;
