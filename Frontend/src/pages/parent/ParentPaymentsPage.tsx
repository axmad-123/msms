import { useEffect, useState } from 'react';
import { msmsApi } from '../../api/msmsApi';
import type { LinkedStudent, Payment } from '../../types';
import { PAYMENT_STATUS } from '../../types';
import { Badge, Card, CardBody, EmptyState, PageHeader, Select, TableWrap, Td, Th } from '../../components/ui';

export function ParentPaymentsPage() {
  const [children, setChildren] = useState<LinkedStudent[]>([]);
  const [studentId, setStudentId] = useState('');
  const [payments, setPayments] = useState<Payment[]>([]);

  useEffect(() => {
    msmsApi.parent.children().then((r) => {
      setChildren(r.data);
      if (r.data[0]) setStudentId(r.data[0].id);
    });
  }, []);

  useEffect(() => {
    if (!studentId) return;
    msmsApi.finance.listPayments(studentId).then((r) => setPayments(r.data));
  }, [studentId]);

  return (
    <>
      <PageHeader title="Payments" description="Fee payment history" />
      <Card className="mb-6">
        <CardBody>
          <Select label="Child" value={studentId} onChange={(e) => setStudentId(e.target.value)}>
            {children.map((c) => (
              <option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>
            ))}
          </Select>
        </CardBody>
      </Card>
      <Card>
        <CardBody className="p-0">
          {payments.length === 0 ? (
            <EmptyState message="No payments found." />
          ) : (
            <TableWrap>
              <thead><tr><Th>Date</Th><Th>Period</Th><Th>Amount</Th><Th>Status</Th></tr></thead>
              <tbody>
                {payments.map((p) => (
                  <tr key={p.id}>
                    <Td>{p.paymentDate}</Td>
                    <Td>{p.month}/{p.year}</Td>
                    <Td>${p.amount}</Td>
                    <Td><Badge tone={p.status === 1 ? 'success' : 'default'}>{PAYMENT_STATUS[p.status]}</Badge></Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>
    </>
  );
}
